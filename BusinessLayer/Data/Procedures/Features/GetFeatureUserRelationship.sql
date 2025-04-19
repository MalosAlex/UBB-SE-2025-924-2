CREATE PROCEDURE GetFeatureUserRelationship
    @userId INT,
    @featureId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Feature_User 
    WHERE user_id = @userId AND feature_id = @featureId;
END
GO