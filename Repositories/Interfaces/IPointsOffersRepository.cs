using BusinessLayer.Models;

namespace BusinessLayer.Repositories.Interfaces
{
    public interface IPointsOffersRepository
    {
        List<PointsOffer> PointsOffers { get; }
    }
}