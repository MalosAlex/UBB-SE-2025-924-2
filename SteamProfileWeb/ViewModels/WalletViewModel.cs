using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLayer.Models; // Assuming PointsOffer is here
using BusinessLayer.Services.Interfaces;
using BusinessLayer.Repositories.Interfaces;

namespace SteamProfileWeb.ViewModels
{
    public class WalletViewModel
    {
        private readonly IWalletService _walletService;
        private readonly IPointsOffersRepository _pointsOffersRepository;

        public decimal Balance { get; private set; }
        public int Points { get; private set; }
        public List<PointsOffer> PointsOffers { get; private set; }

        public string BalanceText => $"${Balance:F2}";
        public string PointsText => $"{Points} points";

        // Constructor to be called by the Controller
        public WalletViewModel(IWalletService walletService, IPointsOffersRepository pointsOffersRepository)
        {
            _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
            _pointsOffersRepository = pointsOffersRepository ?? throw new ArgumentNullException(nameof(pointsOffersRepository));
            
            // Initialize data - this might be done in a separate async method in a real scenario
            // or directly if the service calls are synchronous.
            // For now, let's assume RefreshWalletData will be called.
        }

        public void RefreshWalletData()
        {
            // These methods might need to be adapted if they are async or require user context
            // For now, assuming they are available and work similarly.
            // The original ViewModel gets the current user's wallet. This context needs to be handled.
            // For now, let's assume the services handle the current user context.
            Balance = _walletService.GetBalance(); // This likely needs a user identifier
            Points = _walletService.GetPoints();   // This likely needs a user identifier
            PointsOffers = _pointsOffersRepository.PointsOffers; // This might be a general list
        }

        // Placeholder for AddFunds - will need to be adapted for web, possibly taking input from a form
        public void AddFunds(decimal amount)
        {
            if (amount <= 0)
            {
                return;
            }
            // This also needs user context
            _walletService.AddMoney(amount);
            RefreshWalletData();
        }

        // Placeholder for PurchasePoints - will need to be adapted for web
        public async Task<bool> PurchasePoints(PointsOffer pointsOffer)
        {
            // This also needs user context
            bool success = _walletService.TryPurchasePoints(pointsOffer);
            if (success)
            {
                RefreshWalletData();
            }
            return success; // In a web scenario, this might result in a redirect or message
        }
    }
}