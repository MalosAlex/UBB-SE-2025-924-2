CREATE OR ALTER PROCEDURE GetFriendsForUser
    @user_id INT
AS
BEGIN
    SELECT 
        friendship_id,
        user_id,
        friend_id
    FROM Friendships
    WHERE user_id = @user_id;
END
GO 