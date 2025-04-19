CREATE PROCEDURE DeleteUserSessions
    @user_id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM UserSessions WHERE user_id = @user_id;
END;
go