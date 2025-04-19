create TABLE Wallet (
    wallet_id INT PRIMARY KEY identity(1,1),
    user_id INT unique,
    points INT NOT NULL DEFAULT 0,
    money_for_games DECIMAL(10,2) NOT NULL DEFAULT 0.00
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE ON UPDATE CASCADE,
);