CREATE OR ALTER PROCEDURE CreateUserProfile
    @user_id INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO UserProfiles (user_id)
    VALUES (@user_id);

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
    WHERE profile_id = SCOPE_IDENTITY();
END;
GO