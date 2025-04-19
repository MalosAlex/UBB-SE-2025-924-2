CREATE PROCEDURE UnlockAchievement
    @userId INT,
    @achievementId INT
AS
BEGIN
	IF NOT EXISTS (
        SELECT 1 FROM UserAchievements WHERE user_id = @userId AND achievement_id = @achievementId
    )
    BEGIN
		INSERT INTO UserAchievements (user_id, achievement_id, unlocked_at)
		VALUES (@userId, @achievementId, GETDATE());
	END;
END;