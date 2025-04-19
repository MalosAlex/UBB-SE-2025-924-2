CREATE OR ALTER PROCEDURE AddGameToCollection
    @collection_id INT,
    @game_id INT
AS
BEGIN
    INSERT INTO OwnedGames_Collection (collection_id, game_id)
    VALUES (@collection_id, @game_id)
END
GO