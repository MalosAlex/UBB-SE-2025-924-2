CREATE OR ALTER PROCEDURE MakeCollectionPublic
    @user_id INT,
    @collection_id INT
AS
BEGIN
    UPDATE Collections
    SET is_public = 1
    WHERE collection_id = @collection_id AND user_id = @user_id;
END
GO 