CREATE PROCEDURE CreateFeatureUserRelationship
    @userId INT,
    @featureId INT,
    @equipped BIT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Feature_User (user_id, feature_id, equipped)
    VALUES (@userId, @featureId, @equipped);
END
GO