CREATE TABLE Achievements (
    achievement_id INT PRIMARY KEY identity(1,1),
	achievement_name char(30),
	description char(100),
    achievement_type  NVARCHAR(100) NOT NULL,
    points INT NOT NULL CHECK (points >= 0),
	icon_url NVARCHAR(255) CHECK (icon_url LIKE '%.svg' OR icon_url LIKE '%.png' OR icon_url LIKE '%.jpg')
);

----------------------------- USER ACHIEVEMENTS --------------------------------
CREATE TABLE UserAchievements (
    user_id INT NOT NULL,
    achievement_id INT NOT NULL,
    unlocked_at DATETIME DEFAULT GETDATE(),
    PRIMARY KEY (user_id, achievement_id),
    FOREIGN KEY (user_id) REFERENCES Users (user_id) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (achievement_id) REFERENCES Achievements(achievement_id) ON DELETE CASCADE ON UPDATE CASCADE
);