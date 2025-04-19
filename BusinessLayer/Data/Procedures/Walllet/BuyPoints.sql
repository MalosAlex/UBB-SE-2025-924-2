create or alter procedure BuyPoints @price decimal, @numberOfPoints int, @userId int 
as
begin
	update Wallet 
	set points = points + @numberOfPoints
	where user_id = @user_Id;

	update Wallet
	set money_for_games = money_for_games - @price 
	where user_id = @user_Id
end
