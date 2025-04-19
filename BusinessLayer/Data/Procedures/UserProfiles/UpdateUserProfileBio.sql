go
create or alter procedure UpdateUserProfileBio @user_id int, @bio NVARCHAR(1000) as
begin
	update UserProfiles set bio = @bio where user_id = @user_id
end 