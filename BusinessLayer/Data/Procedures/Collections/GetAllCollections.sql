CREATE OR ALTER PROCEDURE GetAllCollections
AS
BEGIN
    SELECT collection_id, user_id, name, cover_picture, is_public, created_at
    FROM Collections
    ORDER BY name;
END
GO
