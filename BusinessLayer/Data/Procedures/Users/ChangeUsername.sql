create or alter procedure ChangeUsername @user_id int, @newUsername char(50) as
begin
	update Users set username = @newUsername where user_id=@user_id
end