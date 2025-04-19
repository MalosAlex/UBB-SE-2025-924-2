DROP TABLE IF EXISTS Feature_User;
DROP TABLE IF EXISTS UserSessions;
DROP TABLE IF EXISTS User_Achievement;
DROP TABLE IF EXISTS OwnedGames_Collection;
DROP TABLE IF EXISTS User_Wallet;
DROP TABLE IF EXISTS Friendships;
DROP TABLE IF EXISTS Wallet;
DROP TABLE IF EXISTS Collections;
DROP TABLE IF EXISTS Features;
DROP TABLE IF EXISTS Achievements;
DROP TABLE IF EXISTS Friendships;
DROP TABLE IF EXISTS OwnedGames;
DROP TABLE IF EXISTS Users;
drop table if exists PasswordResetCodes;

drop procedure if exists CreateUser;
drop procedure if exists GetAllUsers;
drop procedure if exists GetUserByEmail;
DROP PROCEDURE IF EXISTS ValidateResetCode;
DROP PROCEDURE IF EXISTS ResetPassword;
DROP PROCEDURE IF EXISTS GetUserByEmail;
DROP PROCEDURE IF EXISTS StorePasswordResetCode;
DROP PROCEDURE IF EXISTS VerifyResetCode;
drop procedure if exists CleanupResetCodes;

CREATE TABLE Users (
    user_id INT IDENTITY(1,1) PRIMARY KEY,
    username NVARCHAR(50) COLLATE SQL_Latin1_General_CP1254_CS_AS NOT NULL UNIQUE, -- case sensitivity for usernames
    email NVARCHAR(100) COLLATE SQL_Latin1_General_CP1254_CS_AS NOT NULL UNIQUE, -- case sensitivity for emails
    hashed_password NVARCHAR(255) NOT NULL,
    developer BIT NOT NULL DEFAULT 0,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    last_login DATETIME NULL
);

CREATE TABLE UserSessions (
    session_id UNIQUEIDENTIFIER PRIMARY KEY,
    user_id INT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),  
    expires_at DATETIME NOT NULL,
    FOREIGN KEY (user_id) REFERENCES Users(user_id)
);


-- Achievements Table
CREATE TABLE Achievements (
    achievement_id INT PRIMARY KEY identity(1,1),
    type NVARCHAR(100) NOT NULL,
    points INT NOT NULL CHECK (points >= 0),
    icon NVARCHAR(255) CHECK (icon LIKE '%.svg' OR icon LIKE '%.png' OR icon LIKE '%.jpg' OR icon LIKE '%.jpg')
);

-- Features Table
CREATE TABLE Features (
    feature_id INT PRIMARY KEY identity(1,1),
    name NVARCHAR(100) NOT NULL,
    value INT NOT NULL CHECK (value >= 0),
    description NVARCHAR(255),
    type NVARCHAR(50) CHECK (type IN ('frame', 'emoji', 'background', 'pet', 'hat')),
    source NVARCHAR(255) CHECK (source LIKE '%.svg' OR source LIKE '%.png' OR source LIKE '%.jpg')
);

-- Collections Table
CREATE TABLE Collections (
    collection_id INT PRIMARY KEY identity(1,1),
    user_id INT NOT NULL,
    name NVARCHAR(100) NOT NULL CHECK (LEN(name) >= 1 AND LEN(name) <= 100),
    cover_picture NVARCHAR(255) CHECK (cover_picture LIKE '%.svg' OR cover_picture LIKE '%.png' OR cover_picture LIKE '%.jpg'),
    is_public BIT DEFAULT 1,
    created_at DATE DEFAULT CAST(GETDATE() AS DATE),
    FOREIGN KEY (user_id) REFERENCES Users(user_id)
);

GO

CREATE OR ALTER PROCEDURE GetAllCollections
AS
BEGIN
    SELECT collection_id, user_id, name, cover_picture, is_public, created_at
    FROM Collections
    ORDER BY name;
END
GO

-- Wallet Table
create TABLE Wallet (
    wallet_id INT PRIMARY KEY identity(1,1),
    user_id INT unique,
    points INT NOT NULL DEFAULT 0,
    money_for_games DECIMAL(10,2) NOT NULL DEFAULT 0.00
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE ON UPDATE CASCADE,
);

-- OwnedGames Table(mock table, should check OwnedGames team)
CREATE TABLE OwnedGames (
    game_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT NOT NULL,
    title NVARCHAR(100) NOT NULL CHECK (LEN(title) >= 1 AND LEN(title) <= 100),
    description NVARCHAR(MAX),
    cover_picture NVARCHAR(255) CHECK (cover_picture LIKE '%.svg' OR cover_picture LIKE '%.png' OR cover_picture LIKE '%.jpg'),
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE ON UPDATE CASCADE
);

GO

CREATE OR ALTER PROCEDURE GetAllOwnedGames
AS
BEGIN
    SELECT game_id, user_id, title, description, cover_picture
    FROM OwnedGames
    ORDER BY title;
END
GO

-- OwnedGames_Collection Table
CREATE TABLE OwnedGames_Collection (
    collection_id INT NOT NULL,
    game_id INT NOT NULL,
    PRIMARY KEY (collection_id, game_id),
    FOREIGN KEY (collection_id) REFERENCES Collections(collection_id) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (game_id) REFERENCES OwnedGames(game_id) ON DELETE CASCADE ON UPDATE CASCADE
);

GO

CREATE OR ALTER PROCEDURE GetAllOwnedGamesInCollection
AS
BEGIN
    SELECT collection_id, game_id
    FROM OwnedGames_Collection
    ORDER BY collection_id;
END
GO

-- User_Achievement Table
CREATE TABLE User_Achievement (
    user_id INT NOT NULL,
    achievement_id INT NOT NULL,
    date_unlocked DATETIME DEFAULT GETDATE(),
    PRIMARY KEY (user_id, achievement_id),
    FOREIGN KEY (user_id) REFERENCES Users (user_id) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (achievement_id) REFERENCES Achievements(achievement_id) ON DELETE CASCADE ON UPDATE CASCADE
);

-- Feature_User Table
CREATE TABLE Feature_User (
    user_id INT NOT NULL,
    feature_id INT NOT NULL,
    equipped BIT DEFAULT 0,
    PRIMARY KEY (user_id, feature_id),
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (feature_id) REFERENCES Features(feature_id) ON DELETE CASCADE ON UPDATE CASCADE
);

-- Password Reset Codes Table
CREATE TABLE PasswordResetCodes (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    reset_code NVARCHAR(6) NOT NULL,
    expiration_time DATETIME NOT NULL,
    used BIT DEFAULT 0,
	email nvarchar(255),
    FOREIGN KEY (user_id) REFERENCES Users(user_id)
);

INSERT INTO Users (email, username, hashed_password, developer, last_login) VALUES
('alice@example.com', 'AliceGamer', 'hashed_password_1', 1, '2025-03-20 14:25:00'),
('bob@example.com', 'BobTheBuilder', 'hashed_password_2', 0, '2025-03-21 10:12:00'),
('charlie@example.com', 'CharlieX', 'hashed_password_3', 0, '2025-03-22 18:45:00'),
('diana@example.com', 'DianaRocks', 'hashed_password_4', 0, '2025-03-19 22:30:00'),
('eve@example.com', 'Eve99', 'hashed_password_5', 1, '2025-03-23 08:05:00'),
('frank@example.com', 'FrankTheTank', 'hashed_password_6', 0, '2025-03-24 16:20:00'),
('grace@example.com', 'GraceSpeed', 'hashed_password_7', 0, '2025-03-25 11:40:00'),
('harry@example.com', 'HarryWizard', 'hashed_password_8', 0, '2025-03-20 20:15:00'),
('ivy@example.com', 'IvyNinja', 'hashed_password_9', 0, '2025-03-22 09:30:00'),
('jack@example.com', 'JackHacks', 'hashed_password_10', 1, '2025-03-24 23:55:00');

INSERT INTO Users (email, username, hashed_password, developer, last_login) VALUES
('maracocaina77@gmail.com', 'Mara', 'hashed_password_1', 0, '2025-03-20 14:25:00');

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

CREATE PROCEDURE CreateUser
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

CREATE PROCEDURE DeleteUser
    @userId INT
AS
BEGIN
    DELETE FROM Users
    WHERE user_id = @userId;
END 

go

CREATE PROCEDURE GetAllUsers
AS
BEGIN
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
CREATE PROCEDURE GetUserById
    @userId INT
AS
BEGIN
    SELECT 
        user_id,
        username,
        email,
        developer,
        created_at,
        last_login
    FROM Users
    WHERE user_id = @userId;
END 

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


-- Validate Reset Code
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

-- SELECT @result AS VerificationResult;


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

go
CREATE PROCEDURE GetUserByEmail
    @email NVARCHAR(255)
AS
BEGIN
    SELECT * FROM Users
    WHERE email = @email
END

-- Mock data for OwnedGames table

-- SHOOTERS (game_id 1–3)
INSERT INTO OwnedGames (user_id, title, description, cover_picture)
VALUES
(2, 'Call of Duty: MWIII', 'First-person military shooter', '/Assets/Games/codmw3.png'),
(1, 'Overwatch 2', 'Team-based hero shooter', '/Assets/Games/overwatch2.png'),
(1, 'Counter-Strike 2', 'Tactical shooter', '/Assets/Games/cs2.png');

-- SPORTS (game_id 4–6)
INSERT INTO OwnedGames (user_id, title, description, cover_picture)
VALUES
(1, 'FIFA 25', 'Football simulation', '/Assets/Games/fifa25.png'),
(1, 'NBA 2K25', 'Basketball simulation', '/Assets/Games/nba2k25.png'),
(1, 'Tony Hawk Pro Skater', 'Skateboarding sports game', '/Assets/Games/thps.png');

-- CHILL (game_id 7)
INSERT INTO OwnedGames (user_id, title, description, cover_picture)
VALUES
(1, 'Stardew Valley', 'Relaxing farming game', '/Assets/Games/stardewvalley.png');

-- PETS (game_id 8–10)
INSERT INTO OwnedGames (user_id, title, description, cover_picture)
VALUES
(1, 'The Sims 4: Cats & Dogs', 'Life sim with pets', '/Assets/Games/sims4pets.png'),
(2, 'Nintendogs', 'Pet care simulation', '/Assets/Games/nintendogs.png'),
(1, 'Pet Hotel', 'Manage a hotel for pets', '/Assets/Games/pethotel.png');

-- X-Mas (game_id 11)
INSERT INTO OwnedGames (user_id, title, description, cover_picture)
VALUES
(1, 'Christmas Wonderland', 'Festive hidden object game', '/Assets/Games/xmas.png');

select * from OwnedGames;

SELECT COUNT(*) FROM OwnedGames;

-- Assume collection_id 1–6
INSERT INTO Collections (user_id, name, cover_picture, is_public, created_at)
VALUES
(1, 'All Owned Games', '/Assets/Collections/allgames.jpg', 1, '2022-02-21'),
(2, 'Shooters', '/Assets/Collections/shooters.jpg', 1, '2025-03-21'),
(1, 'Sports', '/Assets/Collections/sports.jpg', 1, '2023-03-21'),
(1, 'Chill Games', '/Assets/Collections/chill.jpg', 1, '2024-03-21'),
(2, 'Pets', '/Assets/Collections/pets.jpg', 0, '2025-01-21'),
(1, 'X-Mas', '/Assets/Collections/xmas.jpg', 0, '2025-02-21');

select * from Collections;

SELECT COUNT(*) FROM Collections;


-- All games (collection_id = 1)
INSERT INTO OwnedGames_Collection (collection_id, game_id)
SELECT 1, game_id FROM OwnedGames;

-- Shooters (collection_id = 2)
INSERT INTO OwnedGames_Collection (collection_id, game_id)
VALUES (2, 1), (2, 2), (2, 3);

-- Sports (collection_id = 3)
INSERT INTO OwnedGames_Collection (collection_id, game_id)
VALUES (3, 4), (3, 5), (3, 6);

-- Chill (collection_id = 4)
INSERT INTO OwnedGames_Collection (collection_id, game_id)
VALUES (4, 7);

-- Pets (collection_id = 5)
INSERT INTO OwnedGames_Collection (collection_id, game_id)
VALUES (5, 8), (5, 9), (5, 10);

-- X-Mas (collection_id = 6)
INSERT INTO OwnedGames_Collection (collection_id, game_id)
VALUES (6, 11);

-- Create Friendships table
CREATE TABLE Friendships (
    friendship_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    friend_id INT NOT NULL,
    CONSTRAINT FK_Friendships_User FOREIGN KEY (user_id) REFERENCES Users(user_id),
    CONSTRAINT FK_Friendships_Friend FOREIGN KEY (friend_id) REFERENCES Users(user_id),
    CONSTRAINT UQ_Friendship UNIQUE (user_id, friend_id),
    CONSTRAINT CHK_FriendshipUsers CHECK (user_id != friend_id)
);

-- Create indexes for Friendships table
CREATE INDEX IX_Friendships_UserId ON Friendships(user_id);
CREATE INDEX IX_Friendships_FriendId ON Friendships(friend_id);

-- Add some mock data for testing
INSERT INTO Friendships (user_id, friend_id)
VALUES 
    (1, 2),
    (2, 1);

-- Verify the friendships data
SELECT * FROM Friendships ORDER BY user_id, friend_id;

GO


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
GO

INSERT INTO Features (name, value, description, type, source) VALUES
('Black Hat', 2000, 'An elegant hat', 'hat', 'Assets/Features/Hats/black-hat.png');

INSERT INTO Features (name, value, description, type, source) VALUES
('Pufu', 10, 'Cute doggo', 'pet', 'Assets/Features/Pets/dog.png');
INSERT INTO Features (name, value, description, type, source) VALUES
('Kitty', 8, 'Cute cat', 'pet', 'Assets/Features/Pets/cat.png');

INSERT INTO Features (name, value, description, type, source) VALUES
('Frame', 5, 'Violet frame', 'frame', 'Assets/Features/Frames/frame1.png');

INSERT INTO Features (name, value, description, type, source) VALUES
('Love Emoji', 7, 'lalal', 'emoji', 'Assets/Features/Emojis/love.png');

INSERT INTO Features (name, value, description, type, source) VALUES
('Violet Background', 7, 'Violet Background', 'background', 'Assets/Features/Backgrounds/violet.jpg');

select * from Wallet

-------- WALLET PROCEDURES-------------
go
create or alter procedure GetWalletById @wallet_id int as
begin
	select * from Wallet where @wallet_id = wallet_id
end
go

create or alter procedure WinPoints @amount int, @userId int 
as 
begin
	update  Wallet 
	set points = points + @amount
	where user_id = @userId
end
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


------Create table PointsOffers ----
create table PointsOffers(
    offerId  INT IDENTITY(1,1) PRIMARY KEY,
    numberOfPoints int not null,
    value int not null
);
insert into PointsOffers(numberOfPoints, value) values
(5, 2),
(25, 8), 
(50, 15), 
(100, 20),
(500, 50)

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

--- initialize wallet for first 11 users
exec createWallet @user_id = 1;
exec createWallet @user_id = 2;
exec createWallet @user_id = 3;
exec createWallet @user_id = 4;
exec createWallet @user_id = 5;
exec createWallet @user_id = 6;
exec createWallet @user_id = 7;
exec createWallet @user_id = 8;
exec createWallet @user_id = 9;
exec createWallet @user_id = 10;
exec createWallet @user_id = 11;

select * from Wallet





USE [profile]
GO
/****** Object:  Table [dbo].[UserProfiles]    Script Date: 01.04.2025 12:18:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserProfiles](
	[profile_id] [int] IDENTITY(1,1) NOT NULL,
	[user_id] [int] NOT NULL,
	[profile_picture] [nvarchar](255) NULL,
	[bio] [nvarchar](1000) NULL,
	[equipped_frame] [nvarchar](255) NULL,
	[equipped_hat] [nvarchar](255) NULL,
	[equipped_pet] [nvarchar](255) NULL,
	[equipped_emoji] [nvarchar](255) NULL,
	[last_modified] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[profile_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 01.04.2025 12:18:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[user_id] [int] IDENTITY(1,1) NOT NULL,
	[username] [nvarchar](50) NOT NULL,
	[email] [nvarchar](100) NOT NULL,
	[hashed_password] [nvarchar](255) NOT NULL,
	[developer] [bit] NOT NULL,
	[created_at] [datetime] NOT NULL,
	[last_login] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserSessions]    Script Date: 01.04.2025 12:18:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserSessions](
	[session_id] [uniqueidentifier] NOT NULL,
	[user_id] [int] NOT NULL,
	[created_at] [datetime] NOT NULL,
	[expires_at] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[session_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[UserProfiles] ADD  DEFAULT (getdate()) FOR [last_modified]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((0)) FOR [developer]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[UserSessions] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[UserProfiles]  WITH CHECK ADD FOREIGN KEY([user_id])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[UserSessions]  WITH CHECK ADD FOREIGN KEY([user_id])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[UserProfiles]  WITH CHECK ADD CHECK  (([profile_picture] like '%.svg' OR [profile_picture] like '%.png' OR [profile_picture] like '%.jpg'))
GO
/****** Object:  StoredProcedure [dbo].[CreateSession]    Script Date: 01.04.2025 12:18:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreateSession]
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
GO
/****** Object:  StoredProcedure [dbo].[CreateUser]    Script Date: 01.04.2025 12:18:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreateUser]
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
GO
/****** Object:  StoredProcedure [dbo].[CreateUserProfile]    Script Date: 01.04.2025 12:18:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[CreateUserProfile]
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
/****** Object:  StoredProcedure [dbo].[DeleteSession]    Script Date: 01.04.2025 12:18:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DeleteSession]
    @session_id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM UserSessions WHERE session_id = @session_id;
END; 
GO
/****** Object:  StoredProcedure [dbo].[GetAllUsers]    Script Date: 01.04.2025 12:18:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAllUsers]
AS
BEGIN
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
GO
/****** Object:  StoredProcedure [dbo].[GetSessionById]    Script Date: 01.04.2025 12:18:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetSessionById]
    @session_id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT session_id, user_id, created_at, expires_at
    FROM UserSessions
    WHERE session_id = @session_id;
END 
GO
/****** Object:  StoredProcedure [dbo].[GetUserByEmail]    Script Date: 01.04.2025 12:18:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetUserByEmail]
    @email NVARCHAR(255)
AS
BEGIN
    SELECT * FROM Users
    WHERE email = @email
END
GO
/****** Object:  StoredProcedure [dbo].[GetUserByEmailOrUsername]    Script Date: 01.04.2025 12:18:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetUserByEmailOrUsername]
    @EmailOrUsername NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT user_id, username, email, hashed_password, developer, created_at, last_login
    FROM Users
    WHERE username = @EmailOrUsername OR email = @EmailOrUsername;
END 
GO
/****** Object:  StoredProcedure [dbo].[GetUserById]    Script Date: 01.04.2025 12:18:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetUserById]
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
GO
/****** Object:  StoredProcedure [dbo].[GetUserFromSession]    Script Date: 01.04.2025 12:18:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetUserFromSession]
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
GO
/****** Object:  StoredProcedure [dbo].[GetUserProfileByUserId]    Script Date: 01.04.2025 12:18:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetUserProfileByUserId]
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

GO
/****** Object:  StoredProcedure [dbo].[UpdateLastLogin]    Script Date: 01.04.2025 12:18:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateLastLogin]
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
GO