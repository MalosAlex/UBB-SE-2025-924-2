CREATE PROCEDURE LoginUser
    @EmailOrUsername NVARCHAR(100),
    @Password NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    -- Get user data including password hash
    SELECT user_id,
        username,
        email,
        hashed_password,
        developer,
        created_at,
        last_login
    FROM Users
    WHERE username = @EmailOrUsername OR email = @EmailOrUsername;
END 
go