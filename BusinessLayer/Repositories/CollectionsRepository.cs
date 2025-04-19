using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using BusinessLayer.Data;
using BusinessLayer.Models;
using Microsoft.Data.SqlClient;
using BusinessLayer.Repositories.Interfaces;
using BusinessLayer.Exceptions;

namespace BusinessLayer.Repositories
{
    public class CollectionsRepository : ICollectionsRepository
    {
        // SQL Parameter Names
        private const string ParameterUserIdentifier = "@user_id";
        private const string ParameterCollectionIdentifierCamel = "@collectionId";   // used in GetCollectionById
        private const string ParameterCollectionIdentifierUnderscore = "@collection_id"; // used in other methods
        private const string ParameterGameIdentifier = "@game_id";
        private const string ParameterName = "@name";
        private const string ParameterCoverPicture = "@cover_picture";
        private const string ParameterIsPublic = "@is_public";
        private const string ParameterCreatedAt = "@created_at";

        // Stored Procedure Names
        private const string StoredProcedure_GetAllCollectionsForUser = "GetAllCollectionsForUser";
        private const string StoredProcedure_GetCollectionByIdentifier = "GetCollectionById";
        private const string StoredProcedure_GetGamesInCollection = "GetGamesInCollection";
        private const string StoredProcedure_GetAllGamesForUser = "GetAllGamesForUser";
        private const string StoredProcedure_AddGameToCollection = "AddGameToCollection";
        private const string StoredProcedure_RemoveGameFromCollection = "RemoveGameFromCollection";
        private const string StoredProcedure_MakeCollectionPrivate = "MakeCollectionPrivate";
        private const string StoredProcedure_MakeCollectionPublic = "MakeCollectionPublic";
        private const string StoredProcedure_DeleteCollection = "DeleteCollection";
        private const string StoredProcedure_CreateCollection = "CreateCollection";
        private const string StoredProcedure_UpdateCollection = "UpdateCollection";
        private const string StoredProcedure_GetPublicCollectionsForUser = "GetPublicCollectionsForUser";
        private const string StoredProcedure_GetGamesNotInCollection = "GetGamesNotInCollection";

        // Error messages
        private const string Error_GetCollections_DataBase = "Database error while retrieving collections.";
        private const string Error_GetCollections_Unexpected = "An unexpected error occurred while retrieving collections.";
        private const string Error_GetCollectionById_DataBase = "Database error while retrieving collection by ID.";
        private const string Error_GetCollectionById_Unexpected = "An unexpected error occurred while retrieving collection by ID.";
        private const string Error_GetGamesInCollection_DataBase = "Database error while retrieving games in collection.";
        private const string Error_GetGamesInCollection_Unexpected = "An unexpected error occurred while retrieving games in collection.";
        private const string Error_AddGameToCollection_DataBase = "Database error while adding game to collection.";
        private const string Error_AddGameToCollection_Unexpected = "An unexpected error occurred while adding game to collection.";
        private const string Error_RemoveGameFromCollection_DataBase = "Database error while removing game from collection.";
        private const string Error_RemoveGameFromCollection_Unexpected = "An unexpected error occurred while removing game from collection.";
        private const string Error_MakeCollectionPrivate = "Failed to make collection {0} private for user {1}.";
        private const string Error_MakeCollectionPublic = "Failed to make collection {0} public for user {1}.";
        private const string Error_RemoveCollection = "Failed to remove collection {0} for user {1}.";
        private const string Error_SaveCollection = "Failed to save collection for user {0}.";
        private const string Error_DeleteCollection_DataBase = "Database error while deleting collection.";
        private const string Error_DeleteCollection_Unexpected = "An unexpected error occurred while deleting collection.";
        private const string Error_CreateCollection_DataBase = "Database error while creating collection.";
        private const string Error_CreateCollection_Unexpected = "An unexpected error occurred while creating collection.";
        private const string Error_UpdateCollection_DataBase = "Database error while updating collection.";
        private const string Error_UpdateCollection_Unexpected = "An unexpected error occurred while updating collection.";
        private const string Error_GetPublicCollections_DataBase = "Database error while retrieving public collections.";
        private const string Error_GetPublicCollections_Unexpected = "An unexpected error occurred while retrieving public collections.";
        private const string Error_GetGamesNotInCollection_DataBase = "Database error while getting games not in collection.";
        private const string Error_GetGamesNotInCollection_Unexpected = "An unexpected error occurred while getting games not in collection.";

        // Column Names in DataRows
        private const string ColumnUserIdentifier = "user_id";
        private const string ColumnName = "name";
        private const string ColumnCreatedAt = "created_at";
        private const string ColumnCoverPicture = "cover_picture";
        private const string ColumnIsPublic = "is_public";
        private const string ColumnCollectionId = "collection_id";
        private const string ColumnTitle = "title";
        private const string ColumnDescription = "description";
        private const string ColumnGameId = "game_id";

        private readonly IDataLink dataLink;

        public CollectionsRepository(IDataLink dataLink)
        {
            this.dataLink = dataLink ?? throw new ArgumentNullException(nameof(dataLink));
        }

        public List<Collection> GetAllCollections(int userIdentifier)
        {
            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterUserIdentifier, userIdentifier)
                };

                var resultTable = dataLink.ExecuteReader(StoredProcedure_GetAllCollectionsForUser, sqlParameters);

                if (resultTable == null || resultTable.Rows.Count == 0)
                {
                    return new List<Collection>();
                }

                var collectionsList = MapDataTableToCollections(resultTable);
                return collectionsList;
            }
            catch (SqlException sqlException)
            {
                throw new RepositoryException(Error_GetCollections_DataBase, sqlException);
            }
            catch (Exception generalException)
            {
                throw new RepositoryException(Error_GetCollections_Unexpected, generalException);
            }
        }

        public List<Collection> GetLastThreeCollectionsForUser(int userIdentifier)
        {
            try
            {
                var allUserCollections = GetAllCollections(userIdentifier);

                var lastThreeCollections = allUserCollections
                    .OrderByDescending(collection => collection.CreatedAt)
                    .Take(3)
                    .ToList();

                return lastThreeCollections;
            }
            catch (Exception generalException)
            {
                throw new RepositoryException("An unexpected error occurred while retrieving the last three collections.", generalException);
            }
        }

        public Collection GetCollectionById(int collectionIdentifier, int userIdentifier)
        {
            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterCollectionIdentifierCamel, collectionIdentifier),
                    new SqlParameter(ParameterUserIdentifier, userIdentifier)
                };

                var resultTable = dataLink.ExecuteReader(StoredProcedure_GetCollectionByIdentifier, sqlParameters);

                if (resultTable == null || resultTable.Rows.Count == 0)
                {
                    return null;
                }

                var collection = MapDataRowToCollection(resultTable.Rows[0]);
                return collection;
            }
            catch (SqlException sqlException)
            {
                throw new RepositoryException(Error_GetCollectionById_DataBase, sqlException);
            }
            catch (Exception generalException)
            {
                throw new RepositoryException(Error_GetCollectionById_Unexpected, generalException);
            }
        }

        public List<OwnedGame> GetGamesInCollection(int collectionIdentifier)
        {
            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterCollectionIdentifierUnderscore, collectionIdentifier)
                };

                var resultTable = dataLink.ExecuteReader(StoredProcedure_GetGamesInCollection, sqlParameters);

                if (resultTable == null || resultTable.Rows.Count == 0)
                {
                    return new List<OwnedGame>();
                }

                var gamesInCollection = resultTable.AsEnumerable().Select(row =>
                {
                    var ownedGame = new OwnedGame(
                        Convert.ToInt32(row[ColumnUserIdentifier]),
                        row[ColumnTitle].ToString(),
                        row[ColumnDescription]?.ToString(),
                        row[ColumnCoverPicture]?.ToString());

                    ownedGame.GameId = Convert.ToInt32(row[ColumnGameId]);
                    return ownedGame;
                }).ToList();

                return gamesInCollection;
            }
            catch (SqlException sqlException)
            {
                throw new RepositoryException(Error_GetGamesInCollection_DataBase, sqlException);
            }
            catch (Exception generalException)
            {
                throw new RepositoryException(Error_GetGamesInCollection_Unexpected, generalException);
            }
        }

        public List<OwnedGame> GetGamesInCollection(int collectionIdentifier, int userIdentifier)
        {
            try
            {
                if (collectionIdentifier == 1)
                {
                    var sqlParameters = new SqlParameter[]
                    {
                        new SqlParameter(ParameterUserIdentifier, userIdentifier)
                    };

                    var resultTable = dataLink.ExecuteReader(StoredProcedure_GetAllGamesForUser, sqlParameters);

                    if (resultTable == null || resultTable.Rows.Count == 0)
                    {
                        return new List<OwnedGame>();
                    }

                    var userOwnedGames = resultTable.AsEnumerable().Select(row =>
                    {
                        var ownedGame = new OwnedGame(
                            Convert.ToInt32(row[ColumnUserIdentifier]),
                            row[ColumnTitle].ToString(),
                            row[ColumnDescription]?.ToString(),
                            row[ColumnCoverPicture]?.ToString());

                        ownedGame.GameId = Convert.ToInt32(row[ColumnGameId]);
                        return ownedGame;
                    }).ToList();

                    return userOwnedGames;
                }
                else
                {
                    return GetGamesInCollection(collectionIdentifier);
                }
            }
            catch (SqlException sqlException)
            {
                throw new RepositoryException(Error_GetGamesInCollection_DataBase, sqlException);
            }
            catch (Exception generalException)
            {
                throw new RepositoryException(Error_GetGamesInCollection_Unexpected, generalException);
            }
        }

        public void AddGameToCollection(int collectionIdentifier, int gameIdentifier, int userIdentifier)
        {
            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterCollectionIdentifierUnderscore, collectionIdentifier),
                    new SqlParameter(ParameterGameIdentifier, gameIdentifier)
                };

                dataLink.ExecuteNonQuery(StoredProcedure_AddGameToCollection, sqlParameters);
            }
            catch (SqlException sqlException)
            {
                throw new RepositoryException(Error_AddGameToCollection_DataBase, sqlException);
            }
            catch (Exception generalException)
            {
                throw new RepositoryException(Error_AddGameToCollection_Unexpected, generalException);
            }
        }

        public void RemoveGameFromCollection(int collectionIdentifier, int gameIdentifier)
        {
            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterCollectionIdentifierUnderscore, collectionIdentifier),
                    new SqlParameter(ParameterGameIdentifier, gameIdentifier)
                };

                dataLink.ExecuteNonQuery(StoredProcedure_RemoveGameFromCollection, sqlParameters);
            }
            catch (SqlException sqlException)
            {
                throw new RepositoryException(Error_RemoveGameFromCollection_DataBase, sqlException);
            }
            catch (Exception generalException)
            {
                throw new RepositoryException(Error_RemoveGameFromCollection_Unexpected, generalException);
            }
        }

        public void MakeCollectionPrivateForUser(string userIdentifier, string collectionIdentifier)
        {
            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterUserIdentifier, userIdentifier),
                    new SqlParameter(ParameterCollectionIdentifierUnderscore, collectionIdentifier)
                };

                dataLink.ExecuteReader(StoredProcedure_MakeCollectionPrivate, sqlParameters);
            }
            catch (DatabaseOperationException dbOperationException)
            {
                throw new RepositoryException(string.Format(Error_MakeCollectionPrivate, collectionIdentifier, userIdentifier), dbOperationException);
            }
        }

        public void MakeCollectionPublicForUser(string userIdentifier, string collectionIdentifier)
        {
            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterUserIdentifier, userIdentifier),
                    new SqlParameter(ParameterCollectionIdentifierUnderscore, collectionIdentifier)
                };

                dataLink.ExecuteReader(StoredProcedure_MakeCollectionPublic, sqlParameters);
            }
            catch (DatabaseOperationException dbOperationException)
            {
                throw new RepositoryException(string.Format(Error_MakeCollectionPublic, collectionIdentifier, userIdentifier), dbOperationException);
            }
        }

        public void RemoveCollectionForUser(string userIdentifier, string collectionIdentifier)
        {
            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterUserIdentifier, userIdentifier),
                    new SqlParameter(ParameterCollectionIdentifierUnderscore, collectionIdentifier)
                };

                dataLink.ExecuteReader(StoredProcedure_DeleteCollection, sqlParameters);
            }
            catch (DatabaseOperationException dbOperationException)
            {
                throw new RepositoryException(string.Format(Error_RemoveCollection, collectionIdentifier, userIdentifier), dbOperationException);
            }
        }

        public void SaveCollection(string userIdentifier, Collection collection)
        {
            try
            {
                if (collection.CollectionId == 0)
                {
                    var sqlParameters = new SqlParameter[]
                    {
                        new SqlParameter(ParameterUserIdentifier, userIdentifier),
                        new SqlParameter(ParameterName, collection.CollectionName),
                        new SqlParameter(ParameterCoverPicture, collection.CoverPicture),
                        new SqlParameter(ParameterIsPublic, collection.IsPublic),
                        new SqlParameter(ParameterCreatedAt, collection.CreatedAt.ToDateTime(TimeOnly.MinValue))
                    };

                    dataLink.ExecuteReader(StoredProcedure_CreateCollection, sqlParameters);
                }
                else
                {
                    var sqlParameters = new SqlParameter[]
                    {
                        new SqlParameter(ParameterCollectionIdentifierUnderscore, collection.CollectionId),
                        new SqlParameter(ParameterUserIdentifier, userIdentifier),
                        new SqlParameter(ParameterName, collection.CollectionName),
                        new SqlParameter(ParameterCoverPicture, collection.CoverPicture),
                        new SqlParameter(ParameterIsPublic, collection.IsPublic),
                        new SqlParameter(ParameterCreatedAt, collection.CreatedAt.ToDateTime(TimeOnly.MinValue))
                    };

                    dataLink.ExecuteReader(StoredProcedure_UpdateCollection, sqlParameters);
                }
            }
            catch (DatabaseOperationException dbOperationException)
            {
                throw new RepositoryException(string.Format(Error_SaveCollection, userIdentifier), dbOperationException);
            }
        }

        public void DeleteCollection(int collectionIdentifier, int userIdentifier)
        {
            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterCollectionIdentifierUnderscore, collectionIdentifier),
                    new SqlParameter(ParameterUserIdentifier, userIdentifier)
                };

                dataLink.ExecuteNonQuery(StoredProcedure_DeleteCollection, sqlParameters);
            }
            catch (SqlException sqlException)
            {
                throw new RepositoryException(Error_DeleteCollection_DataBase, sqlException);
            }
            catch (Exception generalException)
            {
                throw new RepositoryException(Error_DeleteCollection_Unexpected, generalException);
            }
        }

        public void CreateCollection(int userIdentifier, string collectionName, string coverPicture, bool isPublic, DateOnly createdAt)
        {
            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterUserIdentifier, userIdentifier),
                    new SqlParameter(ParameterName, collectionName),
                    new SqlParameter(ParameterCoverPicture, coverPicture),
                    new SqlParameter(ParameterIsPublic, isPublic),
                    new SqlParameter(ParameterCreatedAt, createdAt.ToDateTime(TimeOnly.MinValue))
                };

                dataLink.ExecuteNonQuery(StoredProcedure_CreateCollection, sqlParameters);
            }
            catch (SqlException sqlException)
            {
                throw new RepositoryException(Error_CreateCollection_DataBase, sqlException);
            }
            catch (Exception generalException)
            {
                throw new RepositoryException(Error_CreateCollection_Unexpected, generalException);
            }
        }

        public void UpdateCollection(int collectionIdentifier, int userIdentifier, string collectionName, string coverPicture, bool isPublic)
        {
            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterCollectionIdentifierUnderscore, collectionIdentifier),
                    new SqlParameter(ParameterUserIdentifier, userIdentifier),
                    new SqlParameter(ParameterName, collectionName),
                    new SqlParameter(ParameterCoverPicture, coverPicture),
                    new SqlParameter(ParameterIsPublic, isPublic),
                    new SqlParameter(ParameterCreatedAt, DateOnly.FromDateTime(DateTime.Now).ToDateTime(TimeOnly.MinValue))
                };

                dataLink.ExecuteReader(StoredProcedure_UpdateCollection, sqlParameters);
            }
            catch (SqlException sqlException)
            {
                throw new RepositoryException(Error_UpdateCollection_DataBase, sqlException);
            }
            catch (Exception generalException)
            {
                throw new RepositoryException(Error_UpdateCollection_Unexpected, generalException);
            }
        }

        public List<Collection> GetPublicCollectionsForUser(int userIdentifier)
        {
            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterUserIdentifier, userIdentifier)
                };

                var resultTable = dataLink.ExecuteReader(StoredProcedure_GetPublicCollectionsForUser, sqlParameters);

                if (resultTable == null || resultTable.Rows.Count == 0)
                {
                    return new List<Collection>();
                }

                var publicCollections = MapDataTableToCollections(resultTable);
                return publicCollections;
            }
            catch (SqlException sqlException)
            {
                throw new RepositoryException(Error_GetPublicCollections_DataBase, sqlException);
            }
            catch (Exception generalException)
            {
                throw new RepositoryException(Error_GetPublicCollections_Unexpected, generalException);
            }
        }

        public List<OwnedGame> GetGamesNotInCollection(int collectionIdentifier, int userIdentifier)
        {
            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterCollectionIdentifierUnderscore, collectionIdentifier),
                    new SqlParameter(ParameterUserIdentifier, userIdentifier)
                };

                var resultTable = dataLink.ExecuteReader(StoredProcedure_GetGamesNotInCollection, sqlParameters);

                if (resultTable == null || resultTable.Rows.Count == 0)
                {
                    return new List<OwnedGame>();
                }

                var unassignedGames = resultTable.AsEnumerable().Select(row =>
                {
                    var ownedGame = new OwnedGame(
                        Convert.ToInt32(row[ColumnUserIdentifier]),
                        row[ColumnTitle].ToString(),
                        row[ColumnDescription]?.ToString(),
                        row[ColumnCoverPicture]?.ToString());
                    ownedGame.GameId = Convert.ToInt32(row[ColumnGameId]);
                    return ownedGame;
                }).ToList();

                return unassignedGames;
            }
            catch (SqlException sqlException)
            {
                throw new RepositoryException(Error_GetGamesNotInCollection_DataBase, sqlException);
            }
            catch (Exception generalException)
            {
                throw new RepositoryException(Error_GetGamesNotInCollection_Unexpected, generalException);
            }
        }

        private static List<Collection> MapDataTableToCollections(DataTable dataTable)
        {
            var collectionList = dataTable.AsEnumerable().Select(row =>
            {
                var collection = new Collection(
                    userId: Convert.ToInt32(row[ColumnUserIdentifier]),
                    collectionName: row[ColumnName].ToString(),
                    createdAt: DateOnly.FromDateTime(Convert.ToDateTime(row[ColumnCreatedAt])),
                    coverPicture: row[ColumnCoverPicture]?.ToString(),
                    isPublic: Convert.ToBoolean(row[ColumnIsPublic]));

                collection.CollectionId = Convert.ToInt32(row[ColumnCollectionId]);
                return collection;
            }).ToList();

            return collectionList;
        }

        private static Collection MapDataRowToCollection(DataRow dataRow)
        {
            var collection = new Collection(
                userId: Convert.ToInt32(dataRow[ColumnUserIdentifier]),
                collectionName: dataRow[ColumnName].ToString(),
                createdAt: DateOnly.FromDateTime(Convert.ToDateTime(dataRow[ColumnCreatedAt])),
                coverPicture: dataRow[ColumnCoverPicture]?.ToString(),
                isPublic: Convert.ToBoolean(dataRow[ColumnIsPublic]));

            collection.CollectionId = Convert.ToInt32(dataRow[ColumnCollectionId]);
            return collection;
        }
    }
}
