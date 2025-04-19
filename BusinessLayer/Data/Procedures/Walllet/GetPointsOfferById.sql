create or alter procedure GetPointsOfferByID @offerId int as
begin
	select numberOfPoints, value from PointsOffers where offerId = @offerId
end