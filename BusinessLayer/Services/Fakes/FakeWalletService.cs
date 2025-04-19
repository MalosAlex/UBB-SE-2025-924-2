using BusinessLayer.Models;
using BusinessLayer.Services.Interfaces;

namespace BusinessLayer.Services.Fakes
{
    public class FakeWalletService : IWalletService
    {
        private decimal walletBalance = 100m;
        private int points = 0;
        public List<string> Actions = new();

        public void AddMoney(decimal amount)
        {
            walletBalance += amount;
        }

        public void AddPoints(int points)
        {
            this.points += points;
        }

        public decimal GetBalance()
        {
            return walletBalance;
        }

        public int GetPoints()
        {
            return points;
        }

        public void CreateWallet(int userId)
        {
        }

        public void PurchasePoints(PointsOffer offer)
        {
            if (walletBalance < offer.Price)
            {
                throw new InvalidOperationException("Insufficient funds");
            }

            walletBalance -= offer.Price;
            points += offer.Points;
        }
        public bool TryPurchasePoints(PointsOffer pointsOffer)
        {
            if (pointsOffer == null)
            {
                return false;
            }

            try
            {
                if (walletBalance >= pointsOffer.Price)
                {
                    PurchasePoints(pointsOffer);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
