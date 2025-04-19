CREATE PROCEDURE GetUnlockedAchievements
    @userId INT
AS
BEGIN
	SELECT a.achievement_id, a.achievement_name, a.description, a.achievement_type, a.points, a.icon_url, ua.unlocked_at
    FROM Achievements a
    INNER JOIN UserAchievements ua ON a.achievement_id = ua.achievement_id
    WHERE ua.user_id = @userId;
END