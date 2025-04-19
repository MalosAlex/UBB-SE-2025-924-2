CREATE PROCEDURE GetUnlockedDataForAchievement
    @user_id INT,
    @achievement_id INT
AS
BEGIN
	SELECT a.achievement_name AS AchievementName, a.description AS AchievementDescription, ua.unlocked_at AS UnlockDate
    FROM UserAchievements ua
    JOIN Achievements a ON ua.achievement_id = a.achievement_id
    WHERE ua.user_id = @user_id AND ua.achievement_id = @achievement_id;
END;