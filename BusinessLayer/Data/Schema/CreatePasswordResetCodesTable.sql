CREATE TABLE PasswordResetCodes (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    reset_code NVARCHAR(6) NOT NULL,
    expiration_time DATETIME NOT NULL,
    used BIT DEFAULT 0,
	email nvarchar(255),
    FOREIGN KEY (user_id) REFERENCES Users(user_id)
);