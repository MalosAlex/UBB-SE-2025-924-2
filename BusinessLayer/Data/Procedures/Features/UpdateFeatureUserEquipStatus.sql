
CREATE PROCEDURE UpdateFeatureUserEquipStatus
    @userId INT,
    @featureId INT,
    @equipped BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Feature_User
    SET equipped = @equipped
    WHERE user_id = @userId AND feature_id = @featureId;
END
GO