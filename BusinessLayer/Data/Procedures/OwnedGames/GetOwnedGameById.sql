CREATE OR ALTER PROCEDURE GetOwnedGameById
    @gameId INT
AS
BEGIN
    SELECT game_id, user_id, title, description, cover_picture
    FROM OwnedGames
    WHERE game_id = @gameId
END
GO 