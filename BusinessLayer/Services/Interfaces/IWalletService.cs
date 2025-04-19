using BusinessLayer.Models;

namespace BusinessLayer.Services.Interfaces
{
    public interface IWalletService
    {
        void CreateWallet(int userIdentifier);
        void PurchasePoints(PointsOffer offer);
        decimal GetBalance();
        int GetPoints();
        void AddMoney(decimal amount);
        void AddPoints(int points);
        bool TryPurchasePoints(PointsOffer pointsOffer);
    }
}
