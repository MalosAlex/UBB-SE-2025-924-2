CREATE OR ALTER PROCEDURE GetAllGamesForUser
    @user_id INT
AS
BEGIN
    SELECT game_id, user_id, title, description, cover_picture
    FROM OwnedGames
    WHERE user_id = @user_id
    ORDER BY title;
END
GO