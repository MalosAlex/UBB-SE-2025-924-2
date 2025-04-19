CREATE PROCEDURE CleanupExpiredSessions
AS
BEGIN
    DELETE FROM UserSessions WHERE expires_at < GETDATE();
END;
