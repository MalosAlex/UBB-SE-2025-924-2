CREATE PROCEDURE GetSessionById
    @session_id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT session_id, user_id, created_at, expires_at
    FROM UserSessions
    WHERE session_id = @session_id;
END 
go