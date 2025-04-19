using System;
using System.Data;
using System.Diagnostics;
using BusinessLayer.Data;
using BusinessLayer.Exceptions;
using BusinessLayer.Models;
using BusinessLayer.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace BusinessLayer.Repositories
{
    public class AchievementsRepository : IAchievementsRepository
    {
        private readonly IDataLink dataLink;

        public AchievementsRepository(IDataLink dataLink)
        {
            if (dataLink == null)
            {
                throw new ArgumentNullException(nameof(dataLink));
            }

            this.dataLink = dataLink;
        }

        public void InsertAchievements()
        {
            try
            {
                dataLink.ExecuteNonQuery("InsertAchievements");
                System.Diagnostics.Debug.WriteLine("InsertAchievements stored procedure executed successfully.");
            }
            catch (Exception exception)
            {
                throw new RepositoryException("An unexpected error occurred while inserting achievements.", exception);
            }
        }

        public bool IsAchievementsTableEmpty()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Executing SQL query to check if achievements table is empty...");
                var result = dataLink.ExecuteScalar<int>("IsAchievementsTableEmpty");
                System.Diagnostics.Debug.WriteLine($"Number of achievements in table: {result}");
                return result == 0;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected error while checking if achievements table is empty: {exception.Message}");
                throw new RepositoryException("An unexpected error occurred while checking if achievements table is empty.", exception);
            }
        }

        public void UpdateAchievementIconUrl(int points, string iconUrl)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@points", points),
                    new SqlParameter("@iconUrl", iconUrl)
                };
                dataLink.ExecuteNonQuery("UpdateAchievementIcon", parameters);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected error while updating achievement icon URL: {exception.Message}");
                throw new RepositoryException("An unexpected error occurred while updating achievement icon URL.", exception);
            }
        }

        public List<Achievement> GetAllAchievements()
        {
            try
            {
                var dataTable = dataLink.ExecuteReader("GetAllAchievements");
                return MapDataTableToAchievements(dataTable);
            }
            catch (Exception exception)
            {
                throw new RepositoryException("An unexpected error occurred while retrieving achievements.", exception);
            }
        }

        public List<Achievement> GetUnlockedAchievementsForUser(int userIdentifier)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@userId", userIdentifier)
                };
                var dataTable = dataLink.ExecuteReader("GetUnlockedAchievements", parameters);
                return MapDataTableToAchievements(dataTable);
            }
            catch (Exception exception)
            {
                throw new RepositoryException("An unexpected error occurred while retrieving unlocked achievements.", exception);
            }
        }

        public void UnlockAchievement(int userIdentifier, int achievementId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@userId", userIdentifier),
                    new SqlParameter("@achievementId", achievementId)
                };
                dataLink.ExecuteNonQuery("UnlockAchievement", parameters);
            }
            catch (Exception exception)
            {
                throw new RepositoryException("An unexpected error occurred while unlocking achievement.", exception);
            }
        }

        public void RemoveAchievement(int userIdentifier, int achievementId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@userId", userIdentifier),
                    new SqlParameter("@achievementId", achievementId)
                };
                dataLink.ExecuteNonQuery("RemoveAchievement", parameters);
            }
            catch (Exception exception)
            {
                throw new RepositoryException("An unexpected error occurred while removing achievement.", exception);
            }
        }

        public AchievementUnlockedData GetUnlockedDataForAchievement(int userIdentifier, int achievementId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@user_id", userIdentifier),
                    new SqlParameter("@achievement_id", achievementId)
                };
                var dataTable = dataLink.ExecuteReader("GetUnlockedDataForAchievement", parameters);
                return dataTable.Rows.Count > 0 ? MapDataRowToAchievementUnlockedData(dataTable.Rows[0]) : null;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {exception.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {exception.StackTrace}");
                throw new RepositoryException("An unexpected error occurred while retrieving achievement data.", exception);
            }
        }

        public bool IsAchievementUnlocked(int userIdentifier, int achievementId)
        {
            try
            {
                var parameters = new SqlParameter[]
            {
                new SqlParameter("@user_id", userIdentifier),
                new SqlParameter("@achievement_id", achievementId)
            };

                int? result = dataLink.ExecuteScalar<int>("IsAchievementUnlocked", parameters);
                return result > 0;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected error during ExecuteScalar operation: {exception.Message}");
                throw new RepositoryException("Error checking if achievement is unlocked.", exception);
            }
        }

        public List<AchievementWithStatus> GetAchievementsWithStatusForUser(int userIdentifier)
        {
            try
            {
                var allAchievements = GetAllAchievements();
                var achievementsWithStatus = new List<AchievementWithStatus>();

                foreach (var achievement in allAchievements)
                {
                    var isUnlocked = IsAchievementUnlocked(userIdentifier, achievement.AchievementId);
                    var unlockedData = GetUnlockedDataForAchievement(userIdentifier, achievement.AchievementId);
                    achievementsWithStatus.Add(new AchievementWithStatus
                    {
                        Achievement = achievement,
                        IsUnlocked = isUnlocked,
                        UnlockedDate = unlockedData?.UnlockDate
                    });
                }

                return achievementsWithStatus;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {exception.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {exception.StackTrace}");
                if (exception.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {exception.InnerException.Message}");
                }
                throw new RepositoryException("Error retrieving achievements with status for user.", exception);
            }
        }

        public int GetNumberOfSoldGames(int userIdentifier)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                new SqlParameter("@user_id", userIdentifier)
                };

                var result = dataLink.ExecuteScalar<int>("GetNumberOfSoldGames", parameters);
                return result;
            }
            catch (Exception exception)
            {
                throw new RepositoryException("An unexpected error occurred while retrieving number of sold games.", exception);
            }
        }

        public int GetFriendshipCount(int userIdentifier)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@user_id", userIdentifier)
                };
                return dataLink.ExecuteScalar<int>("GetFriendshipCountForUser", parameters);
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Unexpected Error: {exception.Message}");
                throw new RepositoryException("An unexpected error occurred while retrieving friendship count.", exception);
            }
        }

        public int GetNumberOfOwnedGames(int userIdentifier)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                new SqlParameter("@user_id", userIdentifier)
                };

                var result = dataLink.ExecuteScalar<int>("GetNumberOfOwnedGames", parameters);
                return result;
            }
            catch (Exception exception)
            {
                throw new RepositoryException("An unexpected error occurred while retrieving number of owned games.", exception);
            }
        }

        public int GetNumberOfReviewsGiven(int userIdentifier)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                new SqlParameter("@user_id", userIdentifier)
                };

                var result = dataLink.ExecuteScalar<int>("GetNumberOfReviewsGiven", parameters);
                return result;
            }
            catch (Exception exception)
            {
                throw new RepositoryException("An unexpected error occurred while retrieving number of reviews given.", exception);
            }
        }

        public int GetNumberOfReviewsReceived(int userIdentifier)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                new SqlParameter("@user_id", userIdentifier)
                };

                var result = dataLink.ExecuteScalar<int>("GetNumberOfReviewsReceived", parameters);
                return result;
            }
            catch (Exception exception)
            {
                throw new RepositoryException("An unexpected error occurred while retrieving number of reviews received.", exception);
            }
        }

        public int GetNumberOfPosts(int userIdentifier)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                new SqlParameter("@user_id", userIdentifier)
                };

                var result = dataLink.ExecuteScalar<int>("GetNumberOfPosts", parameters);
                return result;
            }
            catch (Exception exception)
            {
                throw new RepositoryException("An unexpected error occurred while retrieving number of posts.", exception);
            }
        }

        public int GetYearsOfAcftivity(int userIdentifier)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
            new SqlParameter("@user_id", userIdentifier)
                };

                var createdAt = dataLink.ExecuteScalar<DateTime>("GetUserCreatedAt", parameters);
                var yearsOfActivity = DateTime.Now.Year - createdAt.Year;

                // Adjust for the case where the user hasn't completed the current year
                if (DateTime.Now.DayOfYear < createdAt.DayOfYear)
                {
                    yearsOfActivity--;
                }

                return yearsOfActivity;
            }
            catch (Exception exception)
            {
                throw new RepositoryException("An unexpected error occurred while retrieving years of activity.", exception);
            }
        }

        public int? GetAchievementIdByName(string achievementName)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
            new SqlParameter("@achievementName", achievementName)
                };

                var result = dataLink.ExecuteScalar<int?>("GetAchievementIdByName", parameters);
                System.Diagnostics.Debug.WriteLine($"Achievement ID for name {achievementName}: {result}");
                return result;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected error while retrieving achievement ID: {exception.Message}");
                throw new RepositoryException("An unexpected error occurred while retrieving achievement ID.", exception);
            }
        }

        public bool IsUserDeveloper(int userIdentifier)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
            new SqlParameter("@user_id", userIdentifier)
                };

                var result = dataLink.ExecuteScalar<bool>("IsUserDeveloper", parameters);
                return result;
            }
            catch (Exception exception)
            {
                throw new RepositoryException("An unexpected error occurred while retrieving developer status.", exception);
            }
        }
        private static List<Achievement> MapDataTableToAchievements(DataTable dataTable)
        {
            var achievements = new List<Achievement>();
            foreach (DataRow row in dataTable.Rows)
            {
                achievements.Add(MapDataRowToAchievement(row));
            }
            return achievements;
        }
        private static Achievement MapDataRowToAchievement(DataRow row)
        {
            string achievementName = string.Empty;
            string description = string.Empty;
            string achievementType = string.Empty;
            string iconUrl = string.Empty;

            if (row["achievement_name"] != DBNull.Value)
            {
                achievementName = row["achievement_name"].ToString();
            }

            if (row["description"] != DBNull.Value)
            {
                description = row["description"].ToString();
            }

            if (row["achievement_type"] != DBNull.Value)
            {
                achievementType = row["achievement_type"].ToString();
            }

            if (row["icon_url"] != DBNull.Value)
            {
                iconUrl = row["icon_url"].ToString();
            }

            return new Achievement
            {
                AchievementId = Convert.ToInt32(row["achievement_id"]),
                AchievementName = achievementName,
                Description = description,
                AchievementType = achievementType,
                Points = Convert.ToInt32(row["points"]),
                Icon = iconUrl
            };
        }

        private static AchievementUnlockedData MapDataRowToAchievementUnlockedData(DataRow row)
        {
            string achievementName = string.Empty;
            string achievementDescription = string.Empty;
            DateTime? unlockDate = null;

            if (row["AchievementName"] != DBNull.Value)
            {
                achievementName = row["AchievementName"].ToString();
            }

            if (row["AchievementDescription"] != DBNull.Value)
            {
                achievementDescription = row["AchievementDescription"].ToString();
            }

            if (row["UnlockDate"] != DBNull.Value)
            {
                unlockDate = Convert.ToDateTime(row["UnlockDate"]);
            }

            return new AchievementUnlockedData
            {
                AchievementName = achievementName,
                AchievementDescription = achievementDescription,
                UnlockDate = unlockDate
            };
        }
    }
}
