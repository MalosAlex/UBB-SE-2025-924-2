using System.Data.SqlClient;
using System;
using Steam_Community.Data;

namespace SteamCommunity.Reviews.Database
{
    public class DatabaseConnection : IDatabaseConnection
    {
        private readonly string connectionString;

        public DatabaseConnection()
        {
            string CONNECTION_STRING = DatabaseConnectionSettings.CONNECTION_STRING;
            connectionString = CONNECTION_STRING;
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public void Connect(SqlConnection connection)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework or simply write to the console)
                Console.WriteLine($"Error connecting to the database: {ex.Message}");
                throw;
            }
        }

        public void Disconnect(SqlConnection connection)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework or simply write to the console)
                Console.WriteLine($"Error disconnecting from the database: {ex.Message}");
                throw;
            }
        }

        public SqlCommand ExecuteQuery(string query, SqlConnection connection)
        {
            try
            {
                return new SqlCommand(query, connection);
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework or simply write to the console)
                Console.WriteLine($"Error executing query: {ex.Message}");
                throw;
            }
        }



    }
}