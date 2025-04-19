create or alter procedure ChangePassword @user_id int, @newHashedPassword char(100) as
begin
		update Users set hashed_password = @newHashedPassword where user_id=@user_id
end