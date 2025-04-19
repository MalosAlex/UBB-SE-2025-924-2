CREATE PROCEDURE GetUserByEmailOrUsername
    @EmailOrUsername NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT user_id, username, email, hashed_password, developer, created_at, last_login
    FROM Users
    WHERE username = @EmailOrUsername OR email = @EmailOrUsername;
END 