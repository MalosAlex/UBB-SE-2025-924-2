CREATE PROCEDURE GetGamesNotInCollection
    @collection_id INT,
    @user_id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT og.game_id, og.user_id, og.title, og.description, og.cover_picture
    FROM OwnedGames og
    WHERE og.user_id = @user_id
    AND NOT EXISTS (
        SELECT 1
        FROM OwnedGames_Collection ogc
        WHERE ogc.game_id = og.game_id
        AND ogc.collection_id = @collection_id
    )
    ORDER BY og.title;
END
GO