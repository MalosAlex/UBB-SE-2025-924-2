using System.Data;
using BusinessLayer.Data;
using BusinessLayer.Models;
using Microsoft.Data.SqlClient;
using BusinessLayer.Repositories.Interfaces;

namespace BusinessLayer.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly IDataLink dataLink;

        public SessionRepository(IDataLink dataLink)
        {
            this.dataLink = dataLink ?? throw new ArgumentNullException(nameof(dataLink));
        }

        public SessionDetails CreateSession(int userId)
        {
            // Create parameters for the stored procedure
            string userIdParamName = "@user_id";
            var parameters = new SqlParameter[]
            {
                new SqlParameter(userIdParamName, userId)
            };

            // Execute the stored procedure
            DataTable result = dataLink.ExecuteReader("CreateSession", parameters);

            // Process the result
            if (result.Rows.Count > 0)
            {
                DataRow row = result.Rows[0];
                Guid sessionId = (Guid)row["session_id"];
                int userIdFromRow = (int)row["user_id"];
                DateTime createdAt = (DateTime)row["created_at"];
                DateTime expiresAt = (DateTime)row["expires_at"];

                return new SessionDetails
                {
                    SessionId = sessionId,
                    UserId = userIdFromRow,
                    CreatedAt = createdAt,
                    ExpiresAt = expiresAt
                };
            }

            throw new InvalidOperationException("Failed to create session");
        }

        public void DeleteUserSessions(int userId)
        {
            string userIdParamName = "@user_id";
            var parameters = new SqlParameter[]
            {
                new SqlParameter(userIdParamName, userId)
            };

            dataLink.ExecuteNonQuery("DeleteUserSessions", parameters);
        }

        public void DeleteSession(Guid sessionId)
        {
            string sessionIdParamName = "@session_id";
            var parameters = new SqlParameter[]
            {
                new SqlParameter(sessionIdParamName, sessionId)
            };

            dataLink.ExecuteNonQuery("DeleteSession", parameters);
        }

        public SessionDetails GetSessionById(Guid sessionId)
        {
            string sessionIdParamName = "@session_id";
            var parameters = new SqlParameter[]
            {
                new SqlParameter(sessionIdParamName, sessionId)
            };

            DataTable result = dataLink.ExecuteReader("GetSessionById", parameters);

            if (result.Rows.Count > 0)
            {
                DataRow row = result.Rows[0];
                Guid sessionIdFromRow = (Guid)row["session_id"];
                int userId = (int)row["user_id"];
                DateTime createdAt = (DateTime)row["created_at"];
                DateTime expiresAt = (DateTime)row["expires_at"];

                return new SessionDetails
                {
                    SessionId = sessionIdFromRow,
                    UserId = userId,
                    CreatedAt = createdAt,
                    ExpiresAt = expiresAt
                };
            }

            return null;
        }

        public UserWithSessionDetails GetUserFromSession(Guid sessionId)
        {
            string sessionIdParamName = "@session_id";
            var parameters = new SqlParameter[]
            {
                new SqlParameter(sessionIdParamName, sessionId)
            };

            DataTable result = dataLink.ExecuteReader("GetUserFromSession", parameters);

            if (result.Rows.Count > 0)
            {
                DataRow row = result.Rows[0];
                Guid sessionIdFromRow = sessionId;
                DateTime createdAt = (DateTime)row["created_at"];
                DateTime expiresAt = (DateTime)row["expires_at"];
                int userId = (int)row["user_id"];
                string username = (string)row["username"];
                string email = (string)row["email"];
                bool developer = (bool)row["developer"];
                DateTime userCreatedAt = (DateTime)row["created_at"];
                DateTime? lastLogin = row["last_login"] == DBNull.Value ? null : (DateTime?)row["last_login"];

                return new UserWithSessionDetails
                {
                    SessionId = sessionIdFromRow,
                    CreatedAt = createdAt,
                    ExpiresAt = expiresAt,
                    UserId = userId,
                    Username = username,
                    Email = email,
                    Developer = developer,
                    UserCreatedAt = userCreatedAt,
                    LastLogin = lastLogin
                };
            }

            return null;
        }

        public List<Guid> GetExpiredSessions()
        {
            var expiredSessions = new List<Guid>();

            DataTable result = dataLink.ExecuteReader("GetExpiredSessions", null);

            foreach (DataRow row in result.Rows)
            {
                Guid sessionId = (Guid)row["session_id"];
                expiredSessions.Add(sessionId);
            }

            return expiredSessions;
        }
    }
}
