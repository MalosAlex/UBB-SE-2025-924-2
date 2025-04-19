CREATE PROCEDURE GetNumberOfReviews
    @user_id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) AS NumberOfOwnedGames
    FROM Reviews
    WHERE user_id = @user_id;
END;

