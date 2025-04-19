CREATE PROCEDURE GetAchievementIdByName
    @achievementName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT achievement_id FROM Achievements WHERE achievement_name = @achievementName;
END