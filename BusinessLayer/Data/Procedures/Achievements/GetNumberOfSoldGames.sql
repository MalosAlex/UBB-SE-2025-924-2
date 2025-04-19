CREATE PROCEDURE GetNumberOfSoldGames
    @user_id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) AS NumberOfSoldGames
    FROM SoldGames
    WHERE user_id = @user_id;
END;

