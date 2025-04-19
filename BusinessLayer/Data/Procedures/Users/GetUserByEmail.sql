create or alter procedure GetUserByEmail @email char(50)
as
begin
	SELECT user_id, username, email, hashed_password, developer, created_at, last_login
	from Users
	where @email = email
end
