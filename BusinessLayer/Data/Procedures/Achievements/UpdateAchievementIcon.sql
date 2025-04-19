CREATE PROCEDURE UpdateAchievementIcon
	@points INT,
	@iconUrl NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE Achievements
	SET icon_url = @iconUrl
	WHERE points = @points;
END;