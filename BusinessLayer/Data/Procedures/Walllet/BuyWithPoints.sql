create or alter procedure BuyWithPoints @amount int, @userId int 
as 
begin
	update  Wallet 
	set points = points - @amount
	where user_id = @userId
end