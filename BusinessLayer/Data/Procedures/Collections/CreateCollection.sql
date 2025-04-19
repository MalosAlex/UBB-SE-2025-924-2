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

SELECT * FROM Collections;
