create or alter procedure BuyWithMoney @amount decimal, @userId int 
as 
begin
	update  Wallet 
	set money_for_games = money_for_games - @amount
	where user_id = @userId
end