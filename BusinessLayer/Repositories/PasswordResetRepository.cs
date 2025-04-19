using System.Data;
using BusinessLayer.Data;
using Microsoft.Data.SqlClient;
using BusinessLayer.Exceptions;
using BusinessLayer.Repositories.Interfaces;

namespace BusinessLayer.Repositories
{
    public class PasswordResetRepository : IPasswordResetRepository
    {
        private readonly IDataLink dataLink;

        public PasswordResetRepository(IDataLink dataLink)
        {
            this.dataLink = dataLink ?? throw new ArgumentNullException(nameof(dataLink));
        }

        public void StoreResetCode(int userId, string code, DateTime expiryTime)
        {
            try
            {
                // First, delete any existing reset codes for this user
                var deleteParameters = new SqlParameter[]
                {
                    new SqlParameter("@userId", userId)
                };

                dataLink.ExecuteNonQuery("DeleteExistingResetCodes", deleteParameters);

                // Then store the new reset code
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@userId", userId),
                    new SqlParameter("@resetCode", code),
                    new SqlParameter("@expirationTime", expiryTime)
                };

                dataLink.ExecuteNonQuery("StorePasswordResetCode", parameters);
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException($"Failed to store reset code for user {userId}.", exception);
            }
        }

        public bool VerifyResetCode(string email, string code)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@email", email),
                    new SqlParameter("@resetCode", code)
                };

                // Get the reset code data from database
                DataTable result = dataLink.ExecuteReader("GetResetCodeByEmailAndCode", parameters);

                // Check if the reset code exists and is valid
                if (result.Rows.Count > 0)
                {
                    DataRow row = result.Rows[0];
                    DateTime expirationTime = (DateTime)row["expiration_time"];
                    bool isCodeUsed = (bool)row["used"];

                    // Check if code is valid, unexpired, and unused
                    return !isCodeUsed && expirationTime > DateTime.UtcNow;
                }

                return false;
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException("Failed to verify reset code.", exception);
            }
        }

        public bool ResetPassword(string email, string code, string hashedPassword)
        {
            try
            {
                // Get the user ID for the email and verify the reset code
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@email", email),
                    new SqlParameter("@resetCode", code)
                };
                var userTable = dataLink.ExecuteReader("GetResetCodeByEmailAndCode", parameters);

                if (userTable.Rows.Count == 0)
                {
                    return false;
                }

                int userId = (int)userTable.Rows[0]["user_id"];

                // Update the password and mark the reset code as used
                var updateParameters = new SqlParameter[]
                {
                    new SqlParameter("@userId", userId),
                    new SqlParameter("@newPassword", hashedPassword)
                };

                int rowsAffected = dataLink.ExecuteNonQuery("UpdatePasswordAndMarkResetCodeUsed", updateParameters);
                return rowsAffected > 0;
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException("Failed to reset password.", exception);
            }
        }

        public void CleanupExpiredCodes()
        {
            try
            {
                dataLink.ExecuteNonQuery("CleanupExpiredResetCodes", new SqlParameter[0]);
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException("Failed to cleanup expired reset codes.", exception);
            }
        }
    }
}