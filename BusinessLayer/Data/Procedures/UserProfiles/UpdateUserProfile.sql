CREATE PROCEDURE UpdateUserProfile
    @profile_id INT,
    @user_id INT,
    @profile_picture NVARCHAR(255),
    @bio NVARCHAR(1000),
    @equipped_frame NVARCHAR(255),
    @equipped_hat NVARCHAR(255),
    @equipped_pet NVARCHAR(255),
    @equipped_emoji NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE UserProfiles
    SET 
        profile_picture = @profile_picture,
        bio = @bio,
        equipped_frame = @equipped_frame,
        equipped_hat = @equipped_hat,
        equipped_pet = @equipped_pet,
        equipped_emoji = @equipped_emoji,
        last_modified = GETDATE()
    WHERE profile_id = @profile_id AND user_id = @user_id;

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
    WHERE profile_id = @profile_id;
END; 