create or alter procedure GetAllPointsOffers as 
begin
	select numberOfPoints, value from PointsOffers
end