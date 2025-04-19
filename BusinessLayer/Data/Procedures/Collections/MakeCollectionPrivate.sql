CREATE OR ALTER PROCEDURE MakeCollectionPrivate
    @user_id INT,
    @collection_id INT
AS
BEGIN
    UPDATE Collections
    SET is_public = 0
    WHERE collection_id = @collection_id AND user_id = @user_id;
END
GO 