CREATE PROCEDURE GetExpiredSessions
AS
BEGIN
    SET NOCOUNT ON;
    SELECT session_id
    FROM UserSessions
    WHERE expires_at <= GETDATE();
END;
