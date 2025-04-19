CREATE OR ALTER PROCEDURE RemoveFriend
    @friendship_id INT
AS
BEGIN
    DECLARE @user_id INT;
    DECLARE @friend_id INT;

    -- Get the user_id and friend_id from the friendship
    SELECT @user_id = user_id, @friend_id = friend_id
    FROM Friendships
    WHERE friendship_id = @friendship_id;

    -- Delete both directions of the friendship
    DELETE FROM Friendships
    WHERE (user_id = @user_id AND friend_id = @friend_id)
       OR (user_id = @friend_id AND friend_id = @user_id);
END
GO 