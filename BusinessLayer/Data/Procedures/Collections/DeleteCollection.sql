CREATE OR ALTER PROCEDURE DeleteCollection
    @user_id INT,
    @collection_id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Collections WHERE collection_id = @collection_id AND user_id = @user_id;
END
GO