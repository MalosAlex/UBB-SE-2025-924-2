CREATE PROCEDURE GetUserFriends
    @user_id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        f.friend_id,
        u.username,
        u.email,
        u.profile_picture,
        u.description,
        u.developer,
        u.created_at,
        u.last_login
    FROM Friendships f
    JOIN Users u ON f.friend_id = u.user_id
    WHERE f.user_id = @user_id
    ORDER BY u.username;
END 