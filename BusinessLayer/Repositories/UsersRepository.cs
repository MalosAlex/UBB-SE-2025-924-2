using System.Data;
using BusinessLayer.Data;
using BusinessLayer.Models;
using BusinessLayer.Utils;
using Microsoft.Data.SqlClient;
using BusinessLayer.Repositories.Interfaces;
using BusinessLayer.Exceptions;

namespace BusinessLayer.Repositories
{
    public sealed class UsersRepository : IUsersRepository
    {
        private readonly IDataLink dataLink;

        public UsersRepository(IDataLink datalink)
        {
            this.dataLink = datalink ?? throw new ArgumentNullException(nameof(datalink));
        }

        public List<User> GetAllUsers()
        {
            try
            {
                var dataTable = dataLink.ExecuteReader("GetAllUsers");
                return MapDataTableToUsers(dataTable);
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException("Failed to retrieve users from the database.", exception);
            }
        }

        public User? GetUserById(int userId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@user_id", userId)
                };

                var dataTable = dataLink.ExecuteReader("GetUserById", parameters);
                return dataTable.Rows.Count > 0 ? MapDataRowToUser(dataTable.Rows[0]) : null;
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException($"Failed to retrieve user with ID {userId} from the database.", exception);
            }
        }

        public User UpdateUser(User user)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@user_id", user.UserId),
                    new SqlParameter("@email", user.Email),
                    new SqlParameter("@username", user.Username),
                    new SqlParameter("@developer", user.IsDeveloper)
                };

                var dataTable = dataLink.ExecuteReader("UpdateUser", parameters);
                if (dataTable.Rows.Count == 0)
                {
                    throw new RepositoryException($"User with ID {user.UserId} not found.");
                }

                return MapDataRowToUser(dataTable.Rows[0]);
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException($"Failed to update user with ID {user.UserId}.", exception);
            }
        }

        public User CreateUser(User user)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@email", user.Email),
                    new SqlParameter("@username", user.Username),
                    new SqlParameter("@hashed_password", user.Password),
                    new SqlParameter("@developer", user.IsDeveloper)
                };

                var dataTable = dataLink.ExecuteReader("CreateUser", parameters);
                if (dataTable.Rows.Count == 0)
                {
                    throw new RepositoryException("Failed to create user.");
                }
                return MapDataRowToUser(dataTable.Rows[0]);
            }
            catch (DatabaseOperationException exception)
            {
                Console.WriteLine($"Error creating user: {exception.Message}");
                throw new RepositoryException("Failed to create user.", exception);
            }
        }

        public void DeleteUser(int userId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@user_id", userId)
                };

                dataLink.ExecuteNonQuery("DeleteUser", parameters);
            }
            catch (DatabaseOperationException ex)
            {
                throw new RepositoryException($"Failed to delete user with ID {userId}.", ex);
            }
        }

        public User? VerifyCredentials(string emailOrUsername)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@EmailOrUsername", emailOrUsername),
                };

                var dataTable = dataLink.ExecuteReader("GetUserByEmailOrUsername", parameters);

                if (dataTable.Rows.Count > 0)
                {
                    var user = MapDataRowToUserWithPassword(dataTable.Rows[0]);
                    return user;
                }

                return null;
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException("Failed to verify user credentials.", exception);
            }
        }

        public User? GetUserByEmail(string email)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@email", email)
                };

                var dataTable = dataLink.ExecuteReader("GetUserByEmail", parameters);
                return dataTable.Rows.Count > 0 ? MapDataRowToUser(dataTable.Rows[0]) : null;
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException($"Failed to retrieve user with email {email}.", exception);
            }
        }

        public User? GetUserByUsername(string username)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@username", username)
                };

                var dataTable = dataLink.ExecuteReader("GetUserByUsername", parameters);
                return dataTable.Rows.Count > 0 ? MapDataRowToUser(dataTable.Rows[0]) : null;
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException($"Failed to retrieve user with username {username}.", exception);
            }
        }

        public string CheckUserExists(string email, string username)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
            new SqlParameter("@email", email),
            new SqlParameter("@username", username)
                };

                var dataTable = dataLink.ExecuteReader("CheckUserExists", parameters);
                if (dataTable.Rows.Count > 0)
                {
                    var errorType = dataTable.Rows[0]["ErrorType"];
                    return errorType == DBNull.Value ? null : errorType.ToString();
                }
                return null;
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException("Failed to check if user exists.", exception);
            }
        }

        public void ChangeEmail(int userId, string newEmail)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@user_id", userId),
                    new SqlParameter("@newEmail", newEmail)
                };
                dataLink.ExecuteNonQuery("ChangeEmailForUserId", parameters);
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException($"Failed to change email for user ID {userId}.", exception);
            }
        }
        public void ChangePassword(int userId, string newPassword)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@user_id", userId),
                    new SqlParameter("@newHashedPassword", PasswordHasher.HashPassword(newPassword))
                };
                dataLink.ExecuteNonQuery("ChangePassword", parameters);
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException($"Failed to change password for user ID {userId}.", exception);
            }
        }
        public void ChangeUsername(int userId, string newUsername)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@user_id", userId),
                    new SqlParameter("@newUsername", newUsername)
                };
                dataLink.ExecuteNonQuery("ChangeUsername", parameters);
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException($"Failed to change username for user ID {userId}.", exception);
            }
        }

        public void UpdateLastLogin(int userId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@user_id", userId)
                };

                dataLink.ExecuteNonQuery("UpdateLastLogin", parameters);
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException($"Failed to update last login for user ID {userId}.", exception);
            }
        }

        private List<User> MapDataTableToUsers(DataTable dataTable)
        {
            return dataTable.AsEnumerable()
                .Select(MapDataRowToUser)
                .ToList();
        }

        public User? MapDataRowToUser(DataRow row)
        {
            if (row["user_id"] == DBNull.Value || row["email"] == DBNull.Value || row["username"] == DBNull.Value)
            {
                return null;
            }

            return new User
            {
                UserId = Convert.ToInt32(row["user_id"]),
                Username = row["username"].ToString(),
                Email = row["email"].ToString(),
                IsDeveloper = row["developer"] != DBNull.Value ? Convert.ToBoolean(row["developer"]) : false,
                CreatedAt = row["created_at"] != DBNull.Value ? Convert.ToDateTime(row["created_at"]) : DateTime.MinValue,
                LastLogin = row["last_login"] != DBNull.Value ? row["last_login"] as DateTime? : null
            };
        }

        public User? MapDataRowToUserWithPassword(DataRow row)
        {
            if (row["user_id"] == DBNull.Value || row["email"] == DBNull.Value || row["username"] == DBNull.Value || row["hashed_password"] == DBNull.Value)
            {
                return null;
            }

            var user = new User
            {
                UserId = Convert.ToInt32(row["user_id"]),
                Email = row["email"].ToString(),
                Username = row["username"].ToString(),
                IsDeveloper = row["developer"] != DBNull.Value ? Convert.ToBoolean(row["developer"]) : false,
                CreatedAt = row["created_at"] != DBNull.Value ? Convert.ToDateTime(row["created_at"]) : DateTime.MinValue,
                LastLogin = row["last_login"] != DBNull.Value ? row["last_login"] as DateTime? : null,
                Password = row["hashed_password"].ToString()
            };

            return user;
        }
    }
}