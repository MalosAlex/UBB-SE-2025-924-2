using System.Data;
using BusinessLayer.Data;
using BusinessLayer.Models;
using Microsoft.Data.SqlClient;
using BusinessLayer.Repositories.Interfaces;
using BusinessLayer.Exceptions;

namespace BusinessLayer.Repositories
{
    public class UserProfilesRepository : IUserProfilesRepository
    {
        private readonly IDataLink dataLink;

        public UserProfilesRepository(IDataLink dataLink)
        {
            this.dataLink = dataLink ?? throw new ArgumentNullException(nameof(dataLink));
        }

        public UserProfile? GetUserProfileByUserId(int userId)
        {
            try
            {
                string parameterName = "@user_id";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter(parameterName, userId)
                };

                var dataTable = dataLink.ExecuteReader("GetUserProfileByUserId", parameters);
                return dataTable.Rows.Count > 0 ? MapDataRowToUserProfile(dataTable.Rows[0]) : null;
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException($"Failed to retrieve user profile with ID {userId} from the database.", exception);
            }
        }

        public UserProfile? UpdateProfile(UserProfile profile)
        {
            try
            {
                string profileIdParam = "@profile_id";
                string userIdParam = "@user_id";
                string profilePictureParam = "@profile_picture";
                string bioParam = "@bio";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter(profileIdParam, profile.ProfileId),
                    new SqlParameter(userIdParam, profile.UserId),
                    new SqlParameter(profilePictureParam, (object?)profile.ProfilePicture ?? DBNull.Value),
                    new SqlParameter(bioParam, (object?)profile.Bio ?? DBNull.Value)
                };

                var dataTable = dataLink.ExecuteReader("UpdateUserProfile", parameters);
                return dataTable.Rows.Count > 0 ? MapDataRowToUserProfile(dataTable.Rows[0]) : null;
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException($"Failed to update profile for user {profile.UserId}.", exception);
            }
        }

        public UserProfile? CreateProfile(int userId)
        {
            try
            {
                string parameterName = "@user_id";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter(parameterName, userId)
                };

                var dataTable = dataLink.ExecuteReader("CreateUserProfile", parameters);
                return dataTable.Rows.Count > 0 ? MapDataRowToUserProfile(dataTable.Rows[0]) : null;
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException($"Failed to create profile for user {userId}.", exception);
            }
        }

        public void UpdateProfileBio(int user_id, string bio)
        {
            try
            {
                string userIdParam = "@user_id";
                string bioParam = "@bio";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter(userIdParam, user_id),
                    new SqlParameter(bioParam, bio)
                };
                var dataTable = dataLink.ExecuteReader("UpdateUserProfileBio", parameters);
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException($"Failed to update profile for user {user_id}.", exception);
            }
        }

        public void UpdateProfilePicture(int user_id, string picture)
        {
            try
            {
                string userIdParam = "@user_id";
                string profilePictureParam = "@profile_picture";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter(userIdParam, user_id),
                    new SqlParameter(profilePictureParam, picture)
                };

                var dataTable = dataLink.ExecuteReader("UpdateUserProfilePicture", parameters);
            }
            catch (DatabaseOperationException exception)
            {
                throw new RepositoryException($"Failed to update profile for user {user_id}.", exception);
            }
        }

        private static UserProfile MapDataRowToUserProfile(DataRow row)
        {
            int profileId = Convert.ToInt32(row["profile_id"]);
            int userId = Convert.ToInt32(row["user_id"]);
            string? profilePicture = row["profile_picture"] as string;
            string? bio = row["bio"] as string;
            DateTime lastModified = Convert.ToDateTime(row["last_modified"]);

            return new UserProfile
            {
                ProfileId = profileId,
                UserId = userId,
                ProfilePicture = profilePicture,
                Bio = bio,
                LastModified = lastModified
            };
        }
    }
}
