CREATE PROCEDURE UnequipFeaturesByType
    @userId INT,
    @featureType NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE fu
    SET equipped = 0
    FROM Feature_User fu
    JOIN Features f ON fu.feature_id = f.feature_id
    WHERE fu.user_id = @userId AND f.type = @featureType;
    
    RETURN 1;
END
GO