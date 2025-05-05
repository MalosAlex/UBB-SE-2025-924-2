using BusinessLayer.Models;
using BusinessLayer.Repositories.Interfaces;

namespace BusinessLayer.Repositories
{
    public class PointsOffersRepository : IPointsOffersRepository
    {
        public List<PointsOffer> PointsOffers { get; } = [];

        public PointsOffersRepository()
        {
            PointsOffers.Add(new PointsOffer(2, 5));
            PointsOffers.Add(new PointsOffer(8, 25));
            PointsOffers.Add(new PointsOffer(15, 50));
            PointsOffers.Add(new PointsOffer(20, 100));
            PointsOffers.Add(new PointsOffer(50, 500));
        }
    }
}