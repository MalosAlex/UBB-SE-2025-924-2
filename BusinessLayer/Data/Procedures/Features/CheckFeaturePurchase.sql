CREATE PROCEDURE CheckFeaturePurchase
    @userId INT,
    @featureId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(1)
    FROM Feature_User
    WHERE user_id = @userId 
    AND feature_id = @featureId;
END