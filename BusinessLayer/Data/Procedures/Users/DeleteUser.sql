CREATE PROCEDURE DeleteUser
    @userId INT
AS
BEGIN
    DELETE FROM Users
    WHERE user_id = @userId;
END 