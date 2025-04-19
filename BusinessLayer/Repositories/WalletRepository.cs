using System.Data;
using BusinessLayer.Data;
using BusinessLayer.Models;
using Microsoft.Data.SqlClient;
using BusinessLayer.Exceptions;
using BusinessLayer.Repositories.Interfaces;

namespace BusinessLayer.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly IDataLink dataLink;

        // Define stored procedure names as constants
        private static class StoredProcedures
        {
            public const string GetWalletById = "GetWalletById";
            public const string GetWalletIdByUserId = "GetWalletIdByUserId";
            public const string AddMoney = "AddMoney";
            public const string AddPoints = "AddPoints";
            public const string BuyPoints = "BuyPoints";
            public const string CreateWallet = "CreateWallet";
            public const string RemoveWallet = "RemoveWallet";
        }

        // Define parameter names as constants
        private static class ParameterNames
        {
            public const string WalletId = "@wallet_id";
            public const string UserId = "@user_id";
            public const string Amount = "@amount";
            public const string Price = "@price";
            public const string NumberOfPoints = "@numberOfPoints";
        }

        // Define column names as constants
        private static class ColumnNames
        {
            public const string WalletId = "wallet_id";
            public const string UserId = "user_id";
            public const string Points = "points";
            public const string MoneyForGames = "money_for_games";
        }

        // Define error message templates
        private static class ErrorMessages
        {
            public const string NoWalletFound = "No wallet found for user ID {0}.";
            public const string FailedToRetrieveWallet = "Failed to retrieve wallet with ID {0} from the database.";
            public const string FailedToRetrieveWalletId = "Failed to retrieve wallet ID for user ID {0} from the database.";
        }

        public WalletRepository(IDataLink datalink)
        {
            this.dataLink = datalink ?? throw new ArgumentNullException(nameof(datalink));
        }

        public Wallet GetWallet(int walletId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterNames.WalletId, walletId)
                };
                var dataTable = dataLink.ExecuteReader(StoredProcedures.GetWalletById, parameters);
                return MapDataRowToWallet(dataTable.Rows[0]);
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException(
                    string.Format(ErrorMessages.FailedToRetrieveWallet, walletId),
                    exception);
            }
        }

        public int GetWalletIdByUserId(int userId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                     new SqlParameter(ParameterNames.UserId, userId)
                };
                var dataTable = dataLink.ExecuteReader(StoredProcedures.GetWalletIdByUserId, parameters);
                if (dataTable.Rows.Count > 0)
                {
                    return Convert.ToInt32(dataTable.Rows[0][ColumnNames.WalletId]);
                }
                throw new RepositoryException(string.Format(ErrorMessages.NoWalletFound, userId));
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException(
                    string.Format(ErrorMessages.FailedToRetrieveWalletId, userId),
                    exception);
            }
        }

        private Wallet MapDataRowToWallet(DataRow dataRow)
        {
            return new Wallet
            {
                WalletId = Convert.ToInt32(dataRow[ColumnNames.WalletId]),
                UserId = Convert.ToInt32(dataRow[ColumnNames.UserId]),
                Balance = Convert.ToDecimal(dataRow[ColumnNames.MoneyForGames]),
                Points = Convert.ToInt32(dataRow[ColumnNames.Points]),
            };
        }

        public void AddMoneyToWallet(decimal moneyToAdd, int userId)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter(ParameterNames.Amount, moneyToAdd),
                new SqlParameter(ParameterNames.UserId, userId)
            };
            dataLink.ExecuteReader(StoredProcedures.AddMoney, parameters);
        }

        public void AddPointsToWallet(int pointsToAdd, int userId)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter(ParameterNames.Amount, pointsToAdd),
                new SqlParameter(ParameterNames.UserId, userId)
            };
            dataLink.ExecuteReader(StoredProcedures.AddPoints, parameters);
        }

        public decimal GetMoneyFromWallet(int walletId)
        {
            return GetWallet(walletId).Balance;
        }

        public int GetPointsFromWallet(int walletId)
        {
            return GetWallet(walletId).Points;
        }

        public void PurchasePoints(PointsOffer offer, int userId)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter(ParameterNames.Price, offer.Price),
                new SqlParameter(ParameterNames.NumberOfPoints, offer.Points),
                new SqlParameter(ParameterNames.UserId, userId)
            };
            dataLink.ExecuteReader(StoredProcedures.BuyPoints, parameters);
        }

        public void AddNewWallet(int userId)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterNames.UserId, userId)
                };
                dataLink.ExecuteReader(StoredProcedures.CreateWallet, parameters);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public void RemoveWallet(int userId)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterNames.UserId, userId)
                };
                dataLink.ExecuteReader(StoredProcedures.RemoveWallet, parameters);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }
}