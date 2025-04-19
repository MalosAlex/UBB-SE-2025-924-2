CREATE PROCEDURE GetAllFeatures
    @userId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT f.feature_id, f.name, f.value, f.description, f.type, f.source,
           CASE WHEN fu.equipped = 1 THEN 1 ELSE 0 END as equipped
    FROM Features f
    LEFT JOIN Feature_User fu ON f.feature_id = fu.feature_id 
        AND fu.user_id = @userId
    ORDER BY f.type, f.value DESC;
END