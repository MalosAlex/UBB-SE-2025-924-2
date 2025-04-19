CREATE TABLE Friendships (
    friendship_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    friend_id INT NOT NULL,
    CONSTRAINT FK_Friendships_User FOREIGN KEY (user_id) REFERENCES Users(user_id),
    CONSTRAINT FK_Friendships_Friend FOREIGN KEY (friend_id) REFERENCES Users(user_id),
    CONSTRAINT UQ_Friendship UNIQUE (user_id, friend_id),
    CONSTRAINT CHK_FriendshipUsers CHECK (user_id != friend_id)
);

-- Add indexes for better query performance
CREATE INDEX IX_Friendships_UserId ON Friendships(user_id);
CREATE INDEX IX_Friendships_FriendId ON Friendships(friend_id);
