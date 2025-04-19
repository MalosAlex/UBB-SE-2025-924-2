CREATE OR ALTER PROCEDURE RemoveGameFromCollection
    @collection_id INT,
    @game_id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM OwnedGames_Collection WHERE collection_id = @collection_id AND game_id = @game_id;
END
GO