using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLayer.Models;
using BusinessLayer.Services.Interfaces;
using BusinessLayer.Repositories.Interfaces;

using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SteamProfileWeb.ViewModels
{
    public class WalletViewModel : IValidatableObject
    {
        private readonly IWalletService _walletService;
        private readonly IPointsOffersRepository _pointsOffersRepository;

        public decimal Balance { get; private set; }
        public int Points { get; private set; }
        [ValidateNever]
        public List<PointsOffer> PointsOffers { get; private set; }

        [ValidateNever]
        public List<string> PaymentMethods { get; } = new List<string> { "Credit Card", "PayPal" };

        [Required(ErrorMessage = "Payment method is required.")]
        public string SelectedPaymentMethod { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than $0.00.")]
        public decimal? AmountToAdd { get; set; }

        public string? CardNumber { get; set; }
        public string? ExpiryDate { get; set; }
        public string? CVV { get; set; }

        // PayPal Details
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? PayPalEmail { get; set; }


        public string BalanceText => $"${Balance:F2}";
        public string PointsText => $"{Points} points";

        // Parameterless constructor for model binding
        public WalletViewModel()
        {
            _walletService = null!;
            _pointsOffersRepository = null!;
            PointsOffers = new List<PointsOffer>();
            SelectedPaymentMethod = string.Empty;
        }

        // Constructor to be called by the Controller for initial view rendering or when repopulating after error
        public WalletViewModel(IWalletService walletService, IPointsOffersRepository pointsOffersRepository)
        {
            _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
            _pointsOffersRepository = pointsOffersRepository ?? throw new ArgumentNullException(nameof(pointsOffersRepository));
            PointsOffers = new List<PointsOffer>(); // Initialize PointsOffers to avoid null
            SelectedPaymentMethod = string.Empty;  // Initialize SelectedPaymentMethod to avoid null
        }


        public void RefreshWalletData()
        {
            Balance = _walletService.GetBalance();
            Points = _walletService.GetPoints();
            PointsOffers = _pointsOffersRepository.PointsOffers;
        }

        public void AddFunds(decimal amount)
        {
            if (amount <= 0)
            {
                return;
            }
            _walletService.AddMoney(amount);
            RefreshWalletData();
        }

        public async Task<bool> PurchasePoints(PointsOffer pointsOffer)
        {
            return await Task.Run(() =>
            {
                bool success = _walletService.TryPurchasePoints(pointsOffer);
                if (success)
                {
                    RefreshWalletData();
                }
                return success;
            });
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SelectedPaymentMethod == "Credit Card")
            {
                if (string.IsNullOrWhiteSpace(CardNumber))
                {
                    yield return new ValidationResult("Card Number is required for Credit Card payment.", new[] { nameof(CardNumber) });
                }

                if (string.IsNullOrWhiteSpace(ExpiryDate))
                {
                    yield return new ValidationResult("Expiry Date is required for Credit Card payment.", new[] { nameof(ExpiryDate) });
                }
                else if (!Regex.IsMatch(ExpiryDate, @"^(0[1-9]|1[0-2])\/\d{2}$"))
                {
                    yield return new ValidationResult("Expiry Date must be in MM/YY format (e.g., 06/28).", new[] { nameof(ExpiryDate) });
                }

                if (string.IsNullOrWhiteSpace(CVV))
                {
                    yield return new ValidationResult("CVV is required for Credit Card payment.", new[] { nameof(CVV) });
                }
                else if (!Regex.IsMatch(CVV, @"^\d{3,4}$"))
                {
                    yield return new ValidationResult("CVV must be 3 or 4 digits.", new[] { nameof(CVV) });
                }
            }
            else if (SelectedPaymentMethod == "PayPal")
            {
                if (string.IsNullOrWhiteSpace(PayPalEmail))
                {
                    yield return new ValidationResult("PayPal Email is required for PayPal payment.", new[] { nameof(PayPalEmail) });
                }
            }
        }
    }
}