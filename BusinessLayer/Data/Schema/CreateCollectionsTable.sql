CREATE TABLE Collections (
    collection_id INT PRIMARY KEY identity(1,1),
    user_id INT NOT NULL,
    name NVARCHAR(100) NOT NULL CHECK (LEN(name) >= 1 AND LEN(name) <= 100),
    cover_picture NVARCHAR(255) CHECK (cover_picture LIKE '%.svg' OR cover_picture LIKE '%.png' OR cover_picture LIKE '%.jpg'),
    is_public BIT DEFAULT 1,
    created_at DATE DEFAULT CAST(GETDATE() AS DATE),
    FOREIGN KEY (user_id) REFERENCES Users(user_id)
);