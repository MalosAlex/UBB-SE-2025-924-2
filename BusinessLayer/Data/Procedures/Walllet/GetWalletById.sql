create or alter procedure GetWalletById @wallet_id int as
begin
	select * from Wallet where @wallet_id = wallet_id
end