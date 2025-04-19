CREATE PROCEDURE UnequipFeature
    @userId INT,
    @featureId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Feature_User
    SET equipped = 0
    WHERE user_id = @userId AND feature_id = @featureId;
    
    RETURN 1;
END