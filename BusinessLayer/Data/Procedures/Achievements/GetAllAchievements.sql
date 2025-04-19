CREATE PROCEDURE GetAllAchievements
AS
BEGIN
	SELECT *
    FROM Achievements
    ORDER BY points DESC;
END;