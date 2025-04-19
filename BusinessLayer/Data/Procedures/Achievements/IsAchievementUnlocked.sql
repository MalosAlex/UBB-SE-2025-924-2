CREATE OR ALTER PROCEDURE IsAchievementUnlocked
    @user_id INT,
    @achievement_id INT
AS
BEGIN
    SELECT COUNT(1) as IsUnlocked
    FROM UserAchievements
    WHERE user_id = @user_id
    AND achievement_id = @achievement_id;
END;