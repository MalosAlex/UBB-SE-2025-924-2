CREATE OR ALTER PROCEDURE GetPublicCollectionsForUser
    @user_id INT
AS
BEGIN
    SELECT collection_id, user_id, name, cover_picture, is_public, created_at
    FROM Collections
    WHERE user_id = @user_id AND is_public = 1
    ORDER BY name;
END
GO 