using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using BusinessLayer.Data;
using BusinessLayer.Models;
using Microsoft.Data.SqlClient;
using BusinessLayer.Exceptions;
using BusinessLayer.Repositories.Interfaces;

namespace BusinessLayer.Repositories
{
    public class FriendshipsRepository : IFriendshipsRepository
    {
        // SQL Parameter Names
        private const string ParameterUserIdentifier = "@user_id";
        private const string ParameterFriendIdentifier = "@friend_id";
        private const string ParameterFriendshipIdentifierCamel = "@friendshipId";
        private const string ParameterFriendshipIdentifierUnderscore = "@friendship_id";

        // Stored Procedure Names
        private const string StoredProcedure_GetFriendsForUser = "GetFriendsForUser";
        private const string StoredProcedure_GetUserByIdentifier = "GetUserById";
        private const string StoredProcedure_GetUserProfileByUserIdentifier = "GetUserProfileByUserId";
        private const string StoredProcedure_AddFriend = "AddFriend";
        private const string StoredProcedure_GetFriendshipByIdentifier = "GetFriendshipById";
        private const string StoredProcedure_RemoveFriend = "RemoveFriend";
        private const string StoredProcedure_GetFriendshipCountForUser = "GetFriendshipCountForUser";
        private const string StoredProcedure_GetFriendshipIdentifier = "GetFriendshipId";

        // Error messages
        private const string Error_GetFriendshipsDataBase = "Database error while retrieving friendships.";
        private const string Error_GetFriendshipsUnexpected = "An unexpected error occurred while retrieving friendships.";
        private const string Error_AddFriendshipDataBase = "Database error while adding friendship.";
        private const string Error_AddFriendshipUnexpected = "An unexpected error occurred while adding friendship.";
        private const string Error_UserDoesNotExist = "User with ID {0} does not exist.";
        private const string Error_FriendshipAlreadyExists = "Friendship already exists.";
        private const string Error_GetFriendshipByIdentifierDataBase = "Database error while retrieving friendship by ID.";
        private const string Error_GetFriendshipByIdentifierUnexpected = "An unexpected error occurred while retrieving friendship by ID.";
        private const string Error_RemoveFriendshipDataBase = "Database error while removing friendship.";
        private const string Error_RemoveFriendshipUnexpected = "An unexpected error occurred while removing friendship.";
        private const string Error_GetFriendshipCountDataBase = "Database error while retrieving friendship count.";
        private const string Error_GetFriendshipCountUnexpected = "An unexpected error occurred while retrieving friendship count.";
        private const string Error_GetFriendshipIdentifierDataBase = "Database error while retrieving friendship ID.";
        private const string Error_GetFriendshipIdentifierUnexpected = "An unexpected error occurred while retrieving friendship ID.";

        // Column Names
        private const string ColumnFriendshipIdentifier = "friendship_id";
        private const string ColumnUserIdentifier = "user_id";
        private const string ColumnFriendIdentifier = "friend_id";
        private const string ColumnUsername = "username";
        private const string ColumnProfilePicture = "profile_picture";

        private readonly IDataLink dataLink;

        public FriendshipsRepository(IDataLink dataLink)
        {
            this.dataLink = dataLink ?? throw new ArgumentNullException(nameof(dataLink));
        }

        public List<Friendship> GetAllFriendships(int userIdentifier)
        {
            try
            {
                var storedProcedureParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterUserIdentifier, userIdentifier)
                };

                var friendshipDataTable = dataLink.ExecuteReader(StoredProcedure_GetFriendsForUser, storedProcedureParameters);

                var listOfFriendships = new List<Friendship>();
                foreach (DataRow friendshipDataRow in friendshipDataTable.Rows)
                {
                    var friendship = new Friendship(
                        friendshipId: Convert.ToInt32(friendshipDataRow[ColumnFriendshipIdentifier]),
                        userId: Convert.ToInt32(friendshipDataRow[ColumnUserIdentifier]),
                        friendId: Convert.ToInt32(friendshipDataRow[ColumnFriendIdentifier]));

                    var friendProfileQueryParameters = new SqlParameter[]
                    {
                        new SqlParameter(ParameterUserIdentifier, friendship.FriendId)
                    };

                    var friendUserProfileDataTable = dataLink.ExecuteReader(StoredProcedure_GetUserByIdentifier, friendProfileQueryParameters);
                    if (friendUserProfileDataTable.Rows.Count > 0)
                    {
                        friendship.FriendUsername = friendUserProfileDataTable.Rows[0][ColumnUsername].ToString();

                        var friendProfilePictureQueryParameters = new SqlParameter[]
                        {
                            new SqlParameter(ParameterUserIdentifier, friendship.FriendId)
                        };

                        var friendUserProfilePictureDataTable = dataLink.ExecuteReader(StoredProcedure_GetUserProfileByUserIdentifier, friendProfilePictureQueryParameters);
                        if (friendUserProfilePictureDataTable.Rows.Count > 0)
                        {
                            friendship.FriendProfilePicture = friendUserProfilePictureDataTable.Rows[0][ColumnProfilePicture].ToString();
                        }
                    }

                    listOfFriendships.Add(friendship);
                }

                listOfFriendships = listOfFriendships
                    .OrderBy(friendship => friendship.FriendUsername)
                    .ToList();

                return listOfFriendships;
            }
            catch (SqlException sqlException)
            {
                throw new RepositoryException(Error_GetFriendshipsDataBase, sqlException);
            }
            catch (Exception generalException)
            {
                throw new RepositoryException(Error_GetFriendshipsUnexpected, generalException);
            }
        }

        public void AddFriendship(int userIdentifier, int friendUserIdentifier)
        {
            try
            {
                var userExistenceCheckParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterUserIdentifier, userIdentifier)
                };
                var friendExistenceCheckParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterUserIdentifier, friendUserIdentifier)
                };

                var userRecordDataTable = dataLink.ExecuteReader(StoredProcedure_GetUserByIdentifier, userExistenceCheckParameters);
                var friendRecordDataTable = dataLink.ExecuteReader(StoredProcedure_GetUserByIdentifier, friendExistenceCheckParameters);

                if (userRecordDataTable.Rows.Count == 0)
                {
                    throw new RepositoryException(string.Format(Error_UserDoesNotExist, userIdentifier));
                }

                if (friendRecordDataTable.Rows.Count == 0)
                {
                    throw new RepositoryException(string.Format(Error_UserDoesNotExist, friendUserIdentifier));
                }

                var existingFriendshipsForUser = GetAllFriendships(userIdentifier);
                if (existingFriendshipsForUser.Any(existingFriendship => existingFriendship.FriendId == friendUserIdentifier))
                {
                    throw new RepositoryException(Error_FriendshipAlreadyExists);
                }

                var createFriendshipParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterUserIdentifier, userIdentifier),
                    new SqlParameter(ParameterFriendIdentifier, friendUserIdentifier)
                };
                dataLink.ExecuteNonQuery(StoredProcedure_AddFriend, createFriendshipParameters);
            }
            catch (SqlException sqlException)
            {
                throw new RepositoryException(Error_AddFriendshipDataBase, sqlException);
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (Exception generalException)
            {
                throw new RepositoryException(Error_AddFriendshipUnexpected, generalException);
            }
        }

        public Friendship GetFriendshipById(int friendshipIdentifier)
        {
            try
            {
                var retrieveFriendshipParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterFriendshipIdentifierCamel, friendshipIdentifier)
                };
                var friendshipDataTable = dataLink.ExecuteReader(StoredProcedure_GetFriendshipByIdentifier, retrieveFriendshipParameters);
                return friendshipDataTable.Rows.Count > 0
                    ? MapDataRowToFriendship(friendshipDataTable.Rows[0])
                    : null;
            }
            catch (SqlException sqlException)
            {
                throw new RepositoryException(Error_GetFriendshipByIdentifierDataBase, sqlException);
            }
            catch (Exception generalException)
            {
                throw new RepositoryException(Error_GetFriendshipByIdentifierUnexpected, generalException);
            }
        }

        public void RemoveFriendship(int friendshipIdentifier)
        {
            try
            {
                var deleteFriendshipParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterFriendshipIdentifierUnderscore, friendshipIdentifier)
                };
                dataLink.ExecuteNonQuery(StoredProcedure_RemoveFriend, deleteFriendshipParameters);
            }
            catch (SqlException sqlException)
            {
                throw new RepositoryException(Error_RemoveFriendshipDataBase, sqlException);
            }
            catch (Exception generalException)
            {
                throw new RepositoryException(Error_RemoveFriendshipUnexpected, generalException);
            }
        }

        public int GetFriendshipCount(int userIdentifier)
        {
            try
            {
                var countQueryParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterUserIdentifier, userIdentifier)
                };
                return dataLink.ExecuteScalar<int>(StoredProcedure_GetFriendshipCountForUser, countQueryParameters);
            }
            catch (SqlException sqlException)
            {
                throw new RepositoryException(Error_GetFriendshipCountDataBase, sqlException);
            }
            catch (Exception generalException)
            {
                throw new RepositoryException(Error_GetFriendshipCountUnexpected, generalException);
            }
        }

        public int? GetFriendshipId(int userIdentifier, int friendIdentifier)
        {
            try
            {
                var retrieveFriendshipIdParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterUserIdentifier, userIdentifier),
                    new SqlParameter(ParameterFriendIdentifier, friendIdentifier)
                };
                var friendshipIdentifierResult = dataLink.ExecuteScalar<int?>(StoredProcedure_GetFriendshipIdentifier, retrieveFriendshipIdParameters);
                return friendshipIdentifierResult;
            }
            catch (SqlException sqlException)
            {
                throw new RepositoryException(Error_GetFriendshipIdentifierDataBase, sqlException);
            }
            catch (Exception generalException)
            {
                throw new RepositoryException(Error_GetFriendshipIdentifierUnexpected, generalException);
            }
        }

        private static Friendship MapDataRowToFriendship(DataRow friendshipDataRow)
        {
            return new Friendship(
                friendshipId: Convert.ToInt32(friendshipDataRow[ColumnFriendshipIdentifier]),
                userId: Convert.ToInt32(friendshipDataRow[ColumnUserIdentifier]),
                friendId: Convert.ToInt32(friendshipDataRow[ColumnFriendIdentifier]));
        }
    }
}
