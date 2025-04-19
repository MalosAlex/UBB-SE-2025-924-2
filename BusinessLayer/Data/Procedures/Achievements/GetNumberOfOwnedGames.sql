CREATE PROCEDURE GetNumberOfOwnedGames
    @user_id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) AS NumberOfOwnedGames
    FROM OwnedGames
    WHERE user_id = @user_id;
END;
