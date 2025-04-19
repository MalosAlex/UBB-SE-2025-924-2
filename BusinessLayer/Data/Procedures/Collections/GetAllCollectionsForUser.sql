CREATE OR ALTER PROCEDURE GetAllCollectionsForUser
    @user_id INT
AS
BEGIN
    SELECT collection_id, user_id, name, cover_picture, is_public, created_at
    FROM Collections
    WHERE user_id = @user_id
    ORDER BY created_at ASC;
END
GO 