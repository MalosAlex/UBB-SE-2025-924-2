CREATE PROCEDURE DeleteSession
    @session_id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM UserSessions WHERE session_id = @session_id;
END; 