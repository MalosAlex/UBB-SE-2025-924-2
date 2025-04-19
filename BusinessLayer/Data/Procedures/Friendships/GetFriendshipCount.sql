CREATE OR ALTER PROCEDURE GetFriendshipCount
    @user_id INT
AS
BEGIN
    SELECT COUNT(*) as count
    FROM Friendships
    WHERE user_id = @user_id;
END
GO 

