using System;
using BusinessLayer.Models;
using BusinessLayer.Services.Interfaces;
using BusinessLayer.Exceptions;

namespace BusinessLayer.Services.Proxies
{
    public class WalletServiceProxy : ServiceProxy, IWalletService
    {
        private readonly IUserService userService;

        public WalletServiceProxy(IUserService userService, string baseUrl = "https://localhost:7262/api/")
            : base(baseUrl)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public void CreateWallet(int userIdentifier)
        {
            try
            {
                PostAsync($"Wallet/create/{userIdentifier}", null).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Failed to create wallet: {ex.Message}", ex);
            }
        }

        public void PurchasePoints(PointsOffer offer)
        {
            if (offer == null)
            {
                throw new ArgumentNullException(nameof(offer));
            }

            try
            {
                var userId = userService.GetCurrentUser()?.UserId ??
                    throw new InvalidOperationException("User is not logged in");

                PostAsync($"Wallet/purchase-points/{userId}", offer).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Failed to purchase points: {ex.Message}", ex);
            }
        }

        public decimal GetBalance()
        {
            try
            {
                var userId = userService.GetCurrentUser()?.UserId ??
                    throw new InvalidOperationException("User is not logged in");

                var wallet = GetAsync<WalletInfo>($"Wallet/{userId}").GetAwaiter().GetResult();
                return wallet.Balance;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Failed to get balance: {ex.Message}", ex);
            }
        }

        public int GetPoints()
        {
            try
            {
                var userId = userService.GetCurrentUser()?.UserId ??
                    throw new InvalidOperationException("User is not logged in");

                var wallet = GetAsync<WalletInfo>($"Wallet/{userId}").GetAwaiter().GetResult();
                return wallet.Points;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Failed to get points: {ex.Message}", ex);
            }
        }

        public void AddMoney(decimal amount)
        {
            try
            {
                var userId = userService.GetCurrentUser()?.UserId ??
                    throw new InvalidOperationException("User is not logged in");

                PostAsync("Wallet/add-money", new { UserId = userId, Amount = amount })
                    .GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Failed to add money: {ex.Message}", ex);
            }
        }

        public void AddPoints(int points)
        {
            try
            {
                var userId = userService.GetCurrentUser()?.UserId ??
                    throw new InvalidOperationException("User is not logged in");

                PostAsync("Wallet/add-points", new { UserId = userId, Points = points })
                    .GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Failed to add points: {ex.Message}", ex);
            }
        }

        public bool TryPurchasePoints(PointsOffer pointsOffer)
        {
            if (pointsOffer == null)
            {
                return false;
            }

            try
            {
                var userId = userService.GetCurrentUser()?.UserId ??
                    throw new InvalidOperationException("User is not logged in");

                return PostAsync<bool>("Wallet/try-purchase-points", new
                {
                    UserId = userId,
                    OfferId = pointsOffer.OfferId
                }).GetAwaiter().GetResult();
            }
            catch
            {
                return false;
            }
        }
    }

    // Helper class for wallet information
    public class WalletInfo
    {
        public int UserId { get; set; }
        public decimal Balance { get; set; }
        public int Points { get; set; }
    }
}