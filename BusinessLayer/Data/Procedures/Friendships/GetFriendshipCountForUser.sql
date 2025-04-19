CREATE OR ALTER PROCEDURE GetFriendshipCountForUser
    @user_id INT
AS
BEGIN
    SELECT COUNT(*) as friend_count
    FROM Friendships
    WHERE user_id = @user_id;
END
GO 

