create or alter procedure AddMoney @amount decimal, @user_Id int as
begin 
	update wallet  
	set money_for_games = money_for_games + @amount
	where user_id = @user_Id
end