CREATE PROCEDURE LogoutUser
    @session_id UNIQUEIDENTIFIER
AS
BEGIN
    DELETE FROM UserSessions WHERE session_id = @session_id;
END;
go