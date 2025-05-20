    using BusinessLayer.Services.Interfaces;
using BusinessLayer.Models;
using BusinessLayer.Repositories;
using BusinessLayer.Repositories.Interfaces;
using BusinessLayer.Exceptions;

namespace BusinessLayer.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository walletRepository;
        private readonly IUserService userService;

        public WalletService(IWalletRepository walletRepository, IUserService userService)
        {
            this.walletRepository = walletRepository ?? throw new ArgumentNullException(nameof(walletRepository));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public void AddMoney(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than 0.");
            }
            if (amount > 500)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be greater than 500.");
            }
            walletRepository.AddMoneyToWallet(amount, userService.GetCurrentUser().UserId);
        }

        public void CreditPoints(int userId, int numberOfPoints)
        {
            walletRepository.AddPointsToWallet(numberOfPoints, userId);
        }

        public decimal GetBalance()
        {
            int userIdentifier = userService.GetCurrentUser().UserId;

            try
            {
                int walletIdentifier = walletRepository.GetWalletIdByUserId(userIdentifier);
                return walletRepository.GetMoneyFromWallet(walletIdentifier);
            }
            catch (RepositoryException ex) when (ex.Message.Contains("No wallet found"))
            {
                // No wallet found, create one
                CreateWallet(userIdentifier);
                return 0m; // New wallet has 0 balance
            }
        }

        public int GetPoints()
        {
            int userId = userService.GetCurrentUser().UserId;

            try
            {
                int walletId = walletRepository.GetWalletIdByUserId(userId);
                return walletRepository.GetPointsFromWallet(walletId);
            }
            catch (RepositoryException ex) when (ex.Message.Contains("No wallet found"))
            {
                // No wallet found, create one
                CreateWallet(userId);
                return 0; // New wallet has 0 points
            }
        }

        public void CreateWallet(int userIdentifier)
        {
            walletRepository.AddNewWallet(userIdentifier);
        }
    }
}