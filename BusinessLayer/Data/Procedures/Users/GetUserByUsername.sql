create or alter procedure GetUserByUsername @username char(50)
as
begin
	SELECT user_id, username, email, hashed_password, developer, created_at, last_login
	from Users
	where @username = username
end
