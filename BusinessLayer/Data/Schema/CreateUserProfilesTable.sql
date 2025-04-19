CREATE TABLE UserProfiles (
    profile_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL UNIQUE,
    profile_picture NVARCHAR(255) CHECK (profile_picture LIKE '%.svg' OR profile_picture LIKE '%.png' OR profile_picture LIKE '%.jpg'),
    bio NVARCHAR(1000),
    equipped_frame NVARCHAR(255),
    equipped_hat NVARCHAR(255),
    equipped_pet NVARCHAR(255),
    equipped_emoji NVARCHAR(255),
    last_modified DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES Users(user_id)
);
