go
CREATE PROCEDURE UpdateUser
    @user_id INT,
    @email NVARCHAR(100),
    @username NVARCHAR(50),
    @developer BIT
AS
BEGIN
    UPDATE Users
    SET 
        email = @email,
        username = @username,
        developer = @developer
    WHERE user_id = @user_id;

    SELECT 
        user_id,
        username,
        email,
        developer,
        created_at,
        last_login
    FROM Users
    WHERE user_id = @user_id;
END 