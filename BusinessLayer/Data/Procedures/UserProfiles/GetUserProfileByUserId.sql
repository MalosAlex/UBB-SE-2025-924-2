CREATE or alter PROCEDURE GetUserProfileByUserId
    @user_id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        profile_id,
        user_id,
        profile_picture,
        bio,
        equipped_frame,
        equipped_hat,
        equipped_pet,
        equipped_emoji,
        last_modified
    FROM UserProfiles
    WHERE user_id = @user_id;
END; 