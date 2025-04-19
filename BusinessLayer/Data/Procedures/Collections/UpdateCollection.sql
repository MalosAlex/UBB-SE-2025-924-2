CREATE OR ALTER PROCEDURE UpdateCollection
    @collection_id INT,
    @user_id INT,
    @name NVARCHAR(100),
    @cover_picture NVARCHAR(255),
    @is_public BIT,
    @created_at DATE
AS
BEGIN
    UPDATE Collections
    SET name = @name,
        cover_picture = @cover_picture,
        is_public = @is_public,
        created_at = @created_at
    WHERE collection_id = @collection_id AND user_id = @user_id;
END
GO
