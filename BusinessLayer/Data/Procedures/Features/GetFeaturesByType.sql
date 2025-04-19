CREATE PROCEDURE GetFeaturesByType
    @type NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT feature_id, name, value, description, type, source
    FROM Features
    WHERE type = @type
    ORDER BY value DESC;
END