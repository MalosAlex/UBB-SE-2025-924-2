CREATE PROCEDURE CreateSession
    @user_id INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Create new session with 2-hour expiration
    DECLARE @new_session_id UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO UserSessions (user_id, session_id, created_at, expires_at)
    VALUES (
        @user_id,
        @new_session_id,
        GETDATE(),
        DATEADD(HOUR, 2, GETDATE())
    );
    
    -- Return the session details
    SELECT 
        us.session_id,
        us.created_at,
        us.expires_at,
        u.user_id,
        u.username,
        u.email,
        u.developer,
        u.created_at as user_created_at,
        u.last_login
    FROM UserSessions us
    JOIN Users u ON us.user_id = u.user_id
    WHERE us.session_id = @new_session_id;
END; 