CREATE PROCEDURE RemoveAchievement
    @userId INT,
    @achievementId INT
AS
BEGIN
	DELETE FROM UserAchievements
    WHERE user_id = @userId AND achievement_id = @achievementId;
END;