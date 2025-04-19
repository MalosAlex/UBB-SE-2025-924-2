go
create or alter procedure UpdateUserProfilePicture @user_id int, @profile_picture NVARCHAR(255) as
begin
	update UserProfiles set profile_picture = @profile_picture where user_id = @user_id
end 