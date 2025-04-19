create or alter procedure CreateWallet @user_id int as
begin
	insert into Wallet (user_id, points, money_for_games)
	values (@user_id,0,0)

	update Wallet
	set user_id = wallet_id
	where wallet_id = (select max(wallet_id) from Wallet)
end
