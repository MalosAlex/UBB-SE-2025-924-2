-- Database Stored Procedures
USE FinalSteamDB;

-- Users
go
CREATE or alter PROCEDURE CreateUser
    @username NVARCHAR(50),
    @email NVARCHAR(100),
    @hashed_password NVARCHAR(255),
    @developer BIT
    AS
BEGIN
INSERT INTO Users (username, email, hashed_password, developer)
VALUES (@username, @email, @hashed_password, @developer);

SELECT
    user_id,
    username,
    email,
    hashed_password,
    developer,
    created_at,
    last_login
FROM Users
WHERE user_id = SCOPE_IDENTITY();
END;

go
CREATE or alter PROCEDURE DeleteUser
    @user_id INT
    AS
BEGIN

exec DeleteFriendshipsForUser @user_id =@user_id
DELETE FROM Users
WHERE user_id = @user_id;
END

go
CREATE PROCEDURE GetAllUsers
    AS
BEGIN
    SET NOCOUNT ON;

SELECT
    user_id,
    username,
    email,
    developer,
    created_at,
    last_login
FROM Users
ORDER BY username;
END

Go
create or alter procedure GetUserByEmail @email nvarchar(100)
    as
begin
SELECT user_id, username, email, hashed_password, developer, created_at, last_login
from Users
where @email = email
end

go
CREATE PROCEDURE GetUserByEmailOrUsername
    @EmailOrUsername NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

SELECT user_id, username, email, hashed_password, developer, created_at, last_login
FROM Users
WHERE username = @EmailOrUsername OR email = @EmailOrUsername;
END

go
CREATE or alter PROCEDURE GetUserById
    @user_id INT
    AS
BEGIN
    SET NOCOUNT ON;

SELECT
    user_id,
    username,
    email,
    developer,
    created_at,
    last_login
FROM Users
WHERE user_id = @user_id;
END

go
create or alter procedure GetUserByUsername @username char(50)
    as
begin
SELECT user_id, username, email, hashed_password, developer, created_at, last_login
from Users
where @username = username
end

go
CREATE PROCEDURE CheckUserExists
    @email NVARCHAR(100),
    @username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

-- Check for existing email and username
SELECT
    CASE
        WHEN EXISTS (SELECT 1 FROM Users WHERE Email = @email) THEN 'EMAIL_EXISTS'
        WHEN EXISTS (SELECT 1 FROM Users WHERE Username = @username) THEN 'USERNAME_EXISTS'
        ELSE NULL
        END AS ErrorType;
END;

go
CREATE PROCEDURE UpdateLastLogin
    @user_id INT
AS
BEGIN
    SET NOCOUNT ON;

UPDATE Users
SET last_login = GETDATE()
WHERE user_id = @user_id;

SELECT
    user_id,
    username,
    email,
    developer,
    created_at,
    last_login
FROM Users
WHERE user_id = @user_id;
END

go
go
CREATE PROCEDURE UpdateUser
    @user_id INT,
    @email NVARCHAR(100),
    @username NVARCHAR(50),
    @developer BIT
AS
BEGIN
UPDATE Users
SET
    email = @email,
    username = @username,
    developer = @developer
WHERE user_id = @user_id;

SELECT
    user_id,
    username,
    email,
    developer,
    created_at,
    last_login
FROM Users
WHERE user_id = @user_id;
END

go
Create or alter procedure ChangeEmailForUserId @user_id int, @newEmail char(50) as
begin
update Users
set email = @newEmail
where user_id = @user_id
end

go
create or alter procedure ChangePassword @user_id int, @newHashedPassword char(100) as
begin
update Users set hashed_password = @newHashedPassword where user_id=@user_id
end

go
create or alter procedure ChangeUsername @user_id int, @newUsername char(50) as
begin
update Users set username = @newUsername where user_id=@user_id
end
go

-- User Sessions
go
CREATE PROCEDURE CreateSession
    @user_id INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Delete any existing sessions for this user
DELETE FROM UserSessions WHERE user_id = @user_id;

-- Create new session with 2-hour expiration
INSERT INTO UserSessions (user_id, session_id, created_at, expires_at)
VALUES (
           @user_id,
           NEWID(),
           GETDATE(),
           DATEADD(HOUR, 2, GETDATE())
       );

-- Return the session details
SELECT
    us.session_id,
    us.created_at,
    us.expires_at,
    u.user_id,
    u.username,
    u.email,
    u.developer,
    u.created_at as user_created_at,
    u.last_login
FROM UserSessions us
         JOIN Users u ON us.user_id = u.user_id
WHERE us.user_id = @user_id;
END;

go
CREATE PROCEDURE CleanupExpiredSessions
    AS
BEGIN
DELETE FROM UserSessions WHERE expires_at < GETDATE();
END;
go
CREATE PROCEDURE DeleteSession
    @session_id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
DELETE FROM UserSessions WHERE session_id = @session_id;
END;

go
CREATE PROCEDURE GetSessionById
    @session_id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT session_id, user_id, created_at, expires_at
FROM UserSessions
WHERE session_id = @session_id;
END

go
CREATE PROCEDURE GetUserFromSession
    @session_id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if session exists and is not expired
    IF EXISTS (
        SELECT 1 
        FROM UserSessions 
        WHERE session_id = @session_id 
        AND expires_at > GETDATE()
    )
BEGIN
        -- Return user details
SELECT
    u.user_id,
    u.username,
    u.email,
    u.developer,
    u.created_at,
    u.last_login
FROM UserSessions us
         JOIN Users u ON us.user_id = u.user_id
WHERE us.session_id = @session_id;
END
ELSE
BEGIN
        -- If session is expired or doesn't exist, delete it
DELETE FROM UserSessions WHERE session_id = @session_id;
END
END;

go
CREATE PROCEDURE LoginUser
    @EmailOrUsername NVARCHAR(100),
    @Password NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    -- Get user data including password hash
SELECT user_id,
       username,
       email,
       hashed_password,
       developer,
       created_at,
       last_login
FROM Users
WHERE username = @EmailOrUsername OR email = @EmailOrUsername;
END

go
CREATE PROCEDURE LogoutUser
    @session_id UNIQUEIDENTIFIER
AS
BEGIN
DELETE FROM UserSessions WHERE session_id = @session_id;
END;
go

-- User Profiles
go
create or alter procedure UpdateUserProfileBio @user_id int, @bio NVARCHAR(1000) as
begin
update UserProfiles set bio = @bio where user_id = @user_id
end
go
create or alter procedure UpdateUserProfilePicture @user_id int, @profile_picture NVARCHAR(255) as
begin
update UserProfiles set profile_picture = @profile_picture where user_id = @user_id
end
go
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

go
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

go
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
go

-- Password Reset Codes
go
CREATE PROCEDURE ValidateResetCode
    @email NVARCHAR(255),
    @reset_code NVARCHAR(6)
AS
BEGIN
    DECLARE @isValid BIT = 0;
    
    -- Check if the code exists, is not used, and hasn't expired
    IF EXISTS (
        SELECT 1 
        FROM PasswordResetCodes 
        WHERE email = @email 
        AND reset_code = @reset_code 
        AND used = 0 
        AND expiration_time > GETDATE()
    )
BEGIN
        -- Mark the code as used
UPDATE PasswordResetCodes
SET used = 1
WHERE email = @email
  AND reset_code = @reset_code;

SET @isValid = 1;
END

SELECT @isValid AS isValid;
END
GO


CREATE PROCEDURE StorePasswordResetCode
    @userId int,
    @resetCode nvarchar(6),
    @expirationTime datetime
AS
BEGIN
INSERT INTO PasswordResetCodes (user_id, reset_code, expiration_time)
VALUES (@userId, @resetCode, @expirationTime)
END

go
CREATE PROCEDURE VerifyResetCode
    @email nvarchar(255),
    @resetCode nvarchar(6)
AS
BEGIN
    DECLARE @userId int
SELECT @userId = user_id FROM Users WHERE email = @email

    IF EXISTS (
        SELECT 1 
        FROM PasswordResetCodes 
        WHERE user_id = @userId 
        AND reset_code = @resetCode 
        AND expiration_time > GETUTCDATE()
        AND used = 0
    )
SELECT 1 as Result
    ELSE
SELECT 0 as Result
END

--SELECT @result AS VerificationResult;

go

CREATE PROCEDURE ResetPassword
    @email nvarchar(255),
    @resetCode nvarchar(6),
    @newPassword nvarchar(max)
AS
BEGIN
BEGIN TRANSACTION
    
    DECLARE @userId int
SELECT @userId = user_id FROM Users WHERE email = @email

    IF EXISTS (
        SELECT 1 
        FROM PasswordResetCodes 
        WHERE user_id = @userId 
        AND reset_code = @resetCode 
        AND expiration_time > GETUTCDATE()
        AND used = 0
    )
BEGIN
UPDATE Users
SET hashed_password = @newPassword
WHERE user_id = @userId

--UPDATE PasswordResetCodes 
-- SET used = 1 
-- WHERE user_id = @userId 
--AND reset_code = @resetCode

-- Delete the used reset code
DELETE FROM PasswordResetCodes
WHERE user_id = @UserId
  AND reset_code = @ResetCode

    COMMIT
SELECT 1 as Result
END
ELSE
BEGIN
ROLLBACK
SELECT 0 as Result
END
END
go
CREATE PROCEDURE CleanupResetCodes
    AS
BEGIN
    -- Delete expired codes
DELETE FROM PasswordResetCodes
WHERE expiration_time < GETUTCDATE()
END
GO

-- Wallet

go
create or alter procedure CreateWallet @user_id int as
begin
insert into Wallet (user_id, points, money_for_games)
values (@user_id,0,0)

update Wallet
set user_id = wallet_id
where wallet_id = (select max(wallet_id) from Wallet)
end

go
create or alter procedure AddMoney @amount decimal, @userId int as
begin
update wallet
set money_for_games = money_for_games + @amount
where user_id = @userId
end

go
create or alter procedure BuyPoints @price decimal, @numberOfPoints int, @userId int
    as
begin
update Wallet
set points = points + @numberOfPoints
where user_id = @userId;

update Wallet
set money_for_games = money_for_games - @price
where user_id = @userId
end

go
create or alter procedure BuyWithMoney @amount decimal, @userId int
    as
begin
update  Wallet
set money_for_games = money_for_games - @amount
where user_id = @userId
end

go
create or alter procedure BuyWithPoints @amount int, @userId int
    as
begin
update  Wallet
set points = points - @amount
where user_id = @userId
end

go
create or alter procedure GetAllPointsOffers as
begin
select numberOfPoints, value from PointsOffers
end

go
create or alter procedure GetPointsOfferByID @offerId int as
begin
select numberOfPoints, value from PointsOffers where offerId = @offerId
end

go
create or alter procedure GetWalletById @wallet_id int as
begin
select * from Wallet where @wallet_id = wallet_id
end
go
create or alter procedure GetWalletIdByUserId @user_id int as
begin
select wallet_id from Wallet where @user_id = user_id
end


go
create or alter procedure WinPoints @amount int, @userId int
    as
begin
update  Wallet
set points = points + @amount
where user_id = @userId
end

-- Points Offers

go
create or alter procedure GetAllPointsOffers as
begin
select numberOfPoints, value from PointsOffers
end
go
create or alter procedure GetPointsOfferByID @offerId int as
begin
select numberOfPoints, value from PointsOffers where offerId = @offerId
end
go

-- Friendships
go
CREATE OR ALTER PROCEDURE AddFriend
    @user_id INT,
    @friend_id INT
    AS
BEGIN
INSERT INTO Friendships (user_id, friend_id)
VALUES (@user_id, @friend_id);
END
GO 
create or alter procedure DeleteFriendshipsForUser @user_id int as
begin
delete from Friendships where @user_id = user_id or @user_id = friend_id
end

go
CREATE OR ALTER PROCEDURE GetFriendsForUser
    @user_id INT
    AS
BEGIN
SELECT
    f.friendship_id,
    f.user_id,
    f.friend_id,
    u.username as friend_username,
    p.profile_picture as friend_profile_picture
FROM Friendships f
         JOIN Users u ON f.friend_id = u.user_id
         JOIN UserProfiles p ON p.user_id = f.friend_id
WHERE f.user_id = @user_id
ORDER BY u.username
END
GO 

go
CREATE OR ALTER PROCEDURE GetFriendshipCountForUser
    @user_id INT
    AS
BEGIN
SELECT COUNT(*) as friend_count
FROM Friendships
WHERE user_id = @user_id;
END
GO 

go
CREATE OR ALTER PROCEDURE RemoveFriend
    @friendship_id INT
    AS
BEGIN
DELETE FROM Friendships
WHERE friendship_id = @friendship_id;
END
GO 

go
CREATE OR ALTER PROCEDURE GetAllFriendships
    AS
BEGIN
SELECT
    f.friendship_id,
    f.user_id,
    u1.username as user_username,
    f.friend_id,
    u2.username as friend_username
FROM Friendships f
         JOIN Users u1 ON f.user_id = u1.user_id
         JOIN Users u2 ON f.friend_id = u2.user_id
ORDER BY f.user_id, f.friend_id;
END
go

-- Collections
GO
CREATE OR ALTER PROCEDURE GetAllCollections
    AS
BEGIN
SELECT collection_id, user_id, name, cover_picture, is_public, created_at
FROM Collections
ORDER BY name;
END


go
CREATE OR ALTER PROCEDURE CreateCollection
    @user_id INT,
    @name NVARCHAR(100),
    @cover_picture NVARCHAR(255),
    @is_public BIT,
    @created_at DATE
    AS
BEGIN
	-- Check if user exists
	IF NOT EXISTS (SELECT 1 FROM Users WHERE user_id = @user_id)
BEGIN
		RAISERROR('User not found', 16, 1)
		RETURN
END

	-- Insert new collection
INSERT INTO Collections (
    user_id,
    name,
    cover_picture,
    is_public,
    created_at
)
VALUES (
           @user_id,
           @name,
           @cover_picture,
           @is_public,
           @created_at
       )

-- Return the newly created collection
SELECT
    collection_id,
    user_id,
    name,
    cover_picture,
    is_public,
    created_at
FROM Collections
WHERE collection_id = SCOPE_IDENTITY()
END

go
go
CREATE OR ALTER PROCEDURE DeleteCollection
    @user_id INT,
    @collection_id INT
    AS
BEGIN
    SET NOCOUNT ON;
    -- Delete associated records first (avoid foreign key constraint errors)
DELETE FROM OwnedGames_Collection WHERE collection_id = @collection_id;

-- Now delete the collection
DELETE FROM Collections WHERE collection_id = @collection_id AND user_id = @user_id;
END
GO 

go
CREATE OR ALTER PROCEDURE GetAllCollectionsForUser
    @user_id INT
    AS
BEGIN
SELECT collection_id, user_id, name, cover_picture, is_public, created_at
FROM Collections
WHERE user_id = @user_id
ORDER BY created_at ASC;
END
GO

go
CREATE PROCEDURE GetCollectionById
    @collectionId INT,
    @user_id INT
AS
BEGIN
SELECT
    collection_id,
    user_id,
    name,
    cover_picture,
    is_public,
    created_at
FROM Collections
WHERE collection_id = @collectionId
  AND user_id = @user_id
END


go
CREATE OR ALTER PROCEDURE GetPrivateCollectionsForUser
    @user_id INT
    AS
BEGIN
SELECT collection_id, user_id, name, cover_picture, is_public, created_at
FROM Collections
WHERE user_id = @user_id AND is_public = 0
ORDER BY name;
END
GO 

go
CREATE OR ALTER PROCEDURE GetPublicCollectionsForUser
    @user_id INT
    AS
BEGIN
SELECT collection_id, user_id, name, cover_picture, is_public, created_at
FROM Collections
WHERE user_id = @user_id AND is_public = 1
ORDER BY name;
END
GO 

go
CREATE OR ALTER PROCEDURE MakeCollectionPrivate
    @user_id INT,
    @collection_id INT
    AS
BEGIN
UPDATE Collections
SET is_public = 0
WHERE collection_id = @collection_id AND user_id = @user_id;
END
GO 

go
CREATE OR ALTER PROCEDURE MakeCollectionPublic
    @user_id INT,
    @collection_id INT
    AS
BEGIN
UPDATE Collections
SET is_public = 1
WHERE collection_id = @collection_id AND user_id = @user_id;
END
GO 


go
CREATE OR ALTER PROCEDURE UpdateCollection
    @collection_id INT,
    @user_id INT,
    @name NVARCHAR(100),
    @cover_picture NVARCHAR(255),
    @is_public BIT,
    @created_at DATE
    AS
BEGIN
UPDATE Collections
SET name = @name,
    cover_picture = @cover_picture,
    is_public = @is_public,
    created_at = @created_at
WHERE collection_id = @collection_id AND user_id = @user_id;
END
GO

-- Owned Games Collection
GO
CREATE OR ALTER PROCEDURE AddGameToCollection
    @collection_id INT,
    @game_id INT
    AS
BEGIN
    -- Check if collection exists
    IF NOT EXISTS (SELECT 1 FROM Collections WHERE collection_id = @collection_id)
BEGIN
        RAISERROR('Collection not found', 16, 1)
        RETURN
END

    -- Check if game exists
    IF NOT EXISTS (SELECT 1 FROM OwnedGames WHERE game_id = @game_id)
BEGIN
        RAISERROR('Game not found', 16, 1)
        RETURN
END

    -- Check if game is already in collection
    IF EXISTS (SELECT 1 FROM OwnedGames_Collection WHERE collection_id = @collection_id AND game_id = @game_id)
BEGIN
        RAISERROR('Game is already in collection', 16, 1)
        RETURN
END

    -- Add game to collection
INSERT INTO OwnedGames_Collection (collection_id, game_id)
VALUES (@collection_id, @game_id)
END


go
CREATE OR ALTER PROCEDURE GetAllOwnedGames
    AS
BEGIN
SELECT game_id, user_id, title, description, cover_picture
FROM OwnedGames
ORDER BY title;
END
GO 

CREATE OR ALTER PROCEDURE GetGamesInCollection
    @collection_id INT
    AS
BEGIN
    -- Check if collection exists
    IF NOT EXISTS (SELECT 1 FROM Collections WHERE collection_id = @collection_id)
BEGIN
        RAISERROR('Collection not found', 16, 1)
        RETURN
END

    -- Get the user_id who owns this collection
    DECLARE @user_id INT
SELECT @user_id = user_id FROM Collections WHERE collection_id = @collection_id

                                                 -- If this is the "All Owned Games" collection (collection_id = 1)
    IF @collection_id = 1
BEGIN
        -- Return all games owned by the user (without duplicates)
SELECT DISTINCT og.game_id, og.user_id, og.title, og.description, og.cover_picture
FROM OwnedGames og
WHERE og.user_id = @user_id
ORDER BY og.title
END
ELSE
BEGIN
        -- For other collections, return games from the collection that belong to the user
SELECT og.game_id, og.user_id, og.title, og.description, og.cover_picture
FROM OwnedGames og
         INNER JOIN OwnedGames_Collection ogc ON og.game_id = ogc.game_id
WHERE ogc.collection_id = @collection_id
  AND og.user_id = @user_id
ORDER BY og.title
END
END
GO 

go
CREATE PROCEDURE GetGamesNotInCollection
    @collection_id INT,
    @user_id INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Get all games owned by the user that are not in the specified collection
SELECT og.game_id,
       og.user_id,
       og.title,
       og.description,
       og.cover_picture
FROM OwnedGames og
WHERE og.user_id = @user_id
  AND NOT EXISTS (
    SELECT 1
    FROM OwnedGames_Collection ogc
    WHERE ogc.game_id = og.game_id
      AND ogc.collection_id = @collection_id
)
ORDER BY og.title;
END


go
CREATE OR ALTER PROCEDURE GetOwnedGameById
    @gameId INT
    AS
BEGIN
SELECT game_id, user_id, title, description, cover_picture
FROM OwnedGames
WHERE game_id = @gameId
END
GO 

go
CREATE OR ALTER PROCEDURE RemoveGameFromCollection
    @collection_id INT,
    @game_id INT
    AS
BEGIN
    SET NOCOUNT ON;
DELETE FROM OwnedGames_Collection WHERE collection_id = @collection_id AND game_id = @game_id;
END
GO

-- Feature Users
go
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

go
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


go
CREATE PROCEDURE CheckFeatureOwnership
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

go
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

go
CREATE PROCEDURE EquipFeature
    @userId INT,
    @featureId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if the feature-user relationship exists
    IF EXISTS (SELECT 1 FROM Feature_User WHERE user_id = @userId AND feature_id = @featureId)
BEGIN
        -- Update existing relationship
UPDATE Feature_User
SET equipped = 1
WHERE user_id = @userId AND feature_id = @featureId;
END
ELSE
BEGIN
        -- Create new relationship
INSERT INTO Feature_User (user_id, feature_id, equipped)
VALUES (@userId, @featureId, 1);
END

RETURN 1;
END

go
CREATE PROCEDURE GetAllFeaturesWithOwnership
    @userId INT
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    f.feature_id,
    f.name,
    f.value,
    f.description,
    f.type,
    f.source,
    CASE WHEN fu.feature_id IS NOT NULL THEN 1 ELSE 0 END as is_owned,
    ISNULL(fu.equipped, 0) as equipped
FROM Features f
         LEFT JOIN Feature_User fu ON f.feature_id = fu.feature_id
    AND fu.user_id = @userId
ORDER BY f.type, f.value DESC;
END

go
CREATE PROCEDURE GetUserFeatures
    @userId INT
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    f.feature_id,
    f.name,
    f.value,
    f.description,
    f.type,
    f.source,
    CASE WHEN fu.feature_id IS NOT NULL THEN 1 ELSE 0 END as is_owned,
    ISNULL(fu.equipped, 0) as equipped
FROM Features f
         LEFT JOIN Feature_User fu ON f.feature_id = fu.feature_id
    AND fu.user_id = @userId
ORDER BY f.type, f.value DESC;
END
go
-- Procedure to unequip a feature
CREATE PROCEDURE UnequipFeature
    @userId INT,
    @featureId INT
AS
BEGIN
    SET NOCOUNT ON;

UPDATE Feature_User
SET equipped = 0
WHERE user_id = @userId AND feature_id = @featureId;

RETURN 1;
END

go
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

-- User Achievements
go
CREATE PROCEDURE UnlockAchievement
    @userId INT,
    @achievementId INT
AS
BEGIN
BEGIN
INSERT INTO UserAchievements (user_id, achievement_id, unlocked_at)
VALUES (@userId, @achievementId, GETDATE());
END;
END;


go
CREATE PROCEDURE GetAllAchievements
    AS
BEGIN
SELECT *
FROM Achievements
ORDER BY points DESC;
END;

go
CREATE PROCEDURE GetNumberOfOwnedGames
    @user_id INT
AS
BEGIN
    SET NOCOUNT ON;
SELECT COUNT(*) AS NumberOfOwnedGames
FROM OwnedGames
WHERE user_id = @user_id;
END;


go
CREATE OR ALTER PROCEDURE GetNumberOfReviewsGiven
    @user_id INT
    AS
BEGIN
    SET NOCOUNT ON;
SELECT COUNT(*) AS NumberOfOwnedGames
FROM ReviewsGiven
WHERE user_id = @user_id;
END;

go
CREATE OR ALTER PROCEDURE GetNumberOfReviewsReceived
    @user_id INT
    AS
BEGIN
    SET NOCOUNT ON;
SELECT COUNT(*) AS NumberOfOwnedGames
FROM ReviewsReceived
WHERE user_id = @user_id;
END;

go
CREATE PROCEDURE GetNumberOfSoldGames
    @user_id INT
AS
BEGIN
    SET NOCOUNT ON;
SELECT COUNT(*) AS NumberOfSoldGames
FROM SoldGames
WHERE user_id = @user_id;
END;

go
CREATE PROCEDURE GetNumberOfPosts
    @user_id INT
AS
BEGIN
    SET NOCOUNT ON;
SELECT COUNT(*) AS NumberOfPosts
FROM Posts
WHERE user_id = @user_id;
END;

go
CREATE PROCEDURE GetUnlockedAchievements
    @userId INT
AS
BEGIN
SELECT a.achievement_id, a.achievement_name, a.description, a.achievement_type, a.points, a.icon_url, ua.unlocked_at
FROM Achievements a
         INNER JOIN UserAchievements ua ON a.achievement_id = ua.achievement_id
WHERE ua.user_id = @userId;
END

go
CREATE PROCEDURE GetUnlockedDataForAchievement
    @user_id INT,
    @achievement_id INT
AS
BEGIN
SELECT a.achievement_name AS AchievementName, a.description AS AchievementDescription, ua.unlocked_at AS UnlockDate
FROM UserAchievements ua
         JOIN Achievements a ON ua.achievement_id = a.achievement_id
WHERE ua.user_id = @user_id AND ua.achievement_id = @achievement_id;
END;

go
CREATE PROCEDURE IsAchievementUnlocked
    @user_id INT,
	@achievement_id INT
AS
BEGIN
	IF EXISTS (
        SELECT 1 FROM UserAchievements 
        WHERE user_id = @user_id 
        AND achievement_id = @achievement_id
    )
SELECT 1;
ELSE
SELECT 0;
END;
go
CREATE OR ALTER PROCEDURE IsAchievementUnlocked
    @user_id INT,
    @achievement_id INT
    AS
BEGIN
SELECT COUNT(1) as IsUnlocked
FROM UserAchievements
WHERE user_id = @user_id
  AND achievement_id = @achievement_id;
END;
go
CREATE or alter PROCEDURE RemoveAchievement
    @userId INT,
    @achievementId INT
    AS
BEGIN
DELETE FROM UserAchievements
WHERE user_id = @userId AND achievement_id = @achievementId;
END;
GO


go
CREATE PROCEDURE GetAchievementIdByName
    @achievementName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
SELECT achievement_id FROM Achievements WHERE achievement_name = @achievementName;
END;

go
CREATE OR ALTER PROCEDURE InsertAchievements
    AS
BEGIN
    SET NOCOUNT ON;

INSERT INTO Achievements (achievement_name, description, achievement_type, points, icon_url)
VALUES
    ('FRIENDSHIP1', 'You made a friend, you get a point', 'Friendships', 1, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('FRIENDSHIP2', 'You made 5 friends, you get 3 points', 'Friendships', 3, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('FRIENDSHIP3', 'You made 10 friends, you get 5 points', 'Friendships', 5, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('FRIENDSHIP4', 'You made 50 friends, you get 10 points', 'Friendships', 10, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('FRIENDSHIP5', 'You made 100 friends, you get 15 points', 'Friendships', 15, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('OWNEDGAMES1', 'You own 1 game, you get 1 point', 'Owned Games', 1, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('OWNEDGAMES2', 'You own 5 games, you get 3 points', 'Owned Games', 3, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('OWNEDGAMES3', 'You own 10 games, you get 5 points', 'Owned Games', 5, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('OWNEDGAMES4', 'You own 50 games, you get 10 points', 'Owned Games', 10, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('SOLDGAMES1', 'You sold 1 game, you get 1 point', 'Sold Games', 1, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('SOLDGAMES2', 'You sold 5 games, you get 3 points', 'Sold Games', 3, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('SOLDGAMES3', 'You sold 10 games, you get 5 points', 'Sold Games', 5, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('SOLDGAMES4', 'You sold 50 games, you get 10 points', 'Sold Games', 10, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('REVIEW1', 'You gave 1 review, you get 1 point', 'Number of Reviews Given', 1, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('REVIEW2', 'You gave 5 reviews, you get 3 points', 'Number of Reviews Given', 3, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('REVIEW3', 'You gave 10 reviews, you get 5 points', 'Number of Reviews Given', 5, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('REVIEW4', 'You gave 50 reviews, you get 10 points', 'Number of Reviews Given', 10, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('REVIEWR1', 'You got 1 review, you get 1 point', 'Number of Reviews Received', 1, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('REVIEWR2', 'You got 5 reviews, you get 3 points', 'Number of Reviews Received', 3, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('REVIEWR3', 'You got 10 reviews, you get 5 points', 'Number of Reviews Received', 5, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('REVIEWR4', 'You got 50 reviews, you get 10 points', 'Number of Reviews Received', 10, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('DEVELOPER', 'You are a developer, you get 10 points', 'Developer', 10, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('ACTIVITY1', 'You have been active for 1 year, you get 1 point', 'Years of Activity', 1, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('ACTIVITY2', 'You have been active for 2 years, you get 3 points', 'Years of Activity', 3, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('ACTIVITY3', 'You have been active for 3 years, you get 5 points', 'Years of Activity', 5, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('ACTIVITY4', 'You have been active for 4 years, you get 10 points', 'Years of Activity', 10, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('POSTS1', 'You have made 1 post, you get 1 point', 'Number of Posts', 1, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('POSTS2', 'You have made 5 posts, you get 3 points', 'Number of Posts', 3, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('POSTS3', 'You have made 10 posts, you get 5 points', 'Number of Posts', 5, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('POSTS4', 'You have made 50 posts, you get 10 points', 'Number of Posts', 10, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png')
;
END;

go
CREATE PROCEDURE GetUserCreatedAt
    @user_id INT
AS
BEGIN
SELECT created_at
FROM Users
WHERE user_id = @user_id;
END;

go
CREATE OR ALTER PROCEDURE IsAchievementUnlocked
    @user_id INT,
    @achievement_id INT
    AS
BEGIN
SELECT COUNT(1) as IsUnlocked
FROM UserAchievements
WHERE user_id = @user_id
  AND achievement_id = @achievement_id;
END;

go
CREATE PROCEDURE IsUserDeveloper
    @user_id INT
AS
BEGIN
SELECT developer
FROM Users
WHERE user_id = @user_id;
END;

go
CREATE PROCEDURE IsAchievementsTableEmpty
    AS
BEGIN
SELECT COUNT(1) FROM Achievements
END

select * from Users