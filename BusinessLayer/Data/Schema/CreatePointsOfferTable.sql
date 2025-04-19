create table PointsOffers(
    offerId  INT IDENTITY(1,1) PRIMARY KEY,
    numberOfPoints int not null,
    value int not null
);

insert into PointsOffers(numberOfPoints, value) values
(5, 2),
(25, 8), 
(50, 15), 
(100, 20),
(500, 50)