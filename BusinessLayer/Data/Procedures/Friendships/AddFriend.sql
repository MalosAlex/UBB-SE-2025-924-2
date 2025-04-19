CREATE OR ALTER PROCEDURE AddFriend
    @user_id INT,
    @friend_id INT
AS
BEGIN
    INSERT INTO Friendships (user_id, friend_id)
    SELECT @user_id, @friend_id
    UNION ALL
    SELECT @friend_id, @user_id;
END
GO 