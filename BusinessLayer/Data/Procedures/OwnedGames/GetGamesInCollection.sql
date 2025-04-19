CREATE OR ALTER PROCEDURE GetGamesInCollection
    @collection_id INT
AS
BEGIN
    DECLARE @user_id INT
    SELECT @user_id = user_id FROM Collections WHERE collection_id = @collection_id

    SELECT og.game_id, og.user_id, og.title, og.description, og.cover_picture
    FROM OwnedGames og
    INNER JOIN OwnedGames_Collection ogc ON og.game_id = ogc.game_id
    WHERE ogc.collection_id = @collection_id
    AND og.user_id = @user_id
    ORDER BY og.title;
END
GO 