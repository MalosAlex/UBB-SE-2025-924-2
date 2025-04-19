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
    public class OwnedGamesRepository : IOwnedGamesRepository
    {
        // SQL Parameter Names
        private const string ParameterUserId = "@user_id";
        private const string ParameterGameIdCamel = "@gameId";
        private const string ParameterUserIdCamel = "@userId";
        private const string ParameterGameIdUnderscore = "@game_id";

        // Stored Procedure Names
        private const string StoredProcedure_GetAllOwnedGames = "GetAllOwnedGames";
        private const string StoredProcedure_GetOwnedGameById = "GetOwnedGameById";
        private const string StoredProcedure_RemoveOwnedGame = "RemoveOwnedGame";

        // Error messages
        private const string Error_GetOwnedGamesDataBase = "Database error while retrieving owned games.";
        private const string Error_GetOwnedGamesUnexpected = "An unexpected error occurred while retrieving owned games.";
        private const string Error_GetOwnedGameByIdDataBase = "Database error while retrieving owned game by ID.";
        private const string Error_GetOwnedGameByIdUnexpected = "An unexpected error occurred while retrieving owned game by ID.";
        private const string Error_RemoveOwnedGameDataBase = "Database error while removing owned game.";
        private const string Error_RemoveOwnedGameUnexpected = "An unexpected error occurred while removing owned game.";

        // Column Names
        private const string ColumnUserId = "user_id";
        private const string ColumnTitle = "title";
        private const string ColumnDescription = "description";
        private const string ColumnCoverPicture = "cover_picture";
        private const string ColumnGameId = "game_id";

        private readonly IDataLink dataLink;

        public OwnedGamesRepository(IDataLink dataLink)
        {
            this.dataLink = dataLink ?? throw new ArgumentNullException(nameof(dataLink));
        }

        public List<OwnedGame> GetAllOwnedGames(int userId)
        {
            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterUserId, userId)
                };

                var ownedGamesDataTable = dataLink.ExecuteReader(StoredProcedure_GetAllOwnedGames, sqlParameters);
                var ownedGamesList = MapDataTableToOwnedGames(ownedGamesDataTable);

                return ownedGamesList;
            }
            catch (SqlException sqlException)
            {
                throw new RepositoryException(Error_GetOwnedGamesDataBase, sqlException);
            }
            catch (Exception generalException)
            {
                throw new RepositoryException(Error_GetOwnedGamesUnexpected, generalException);
            }
        }

        public OwnedGame GetOwnedGameById(int gameId, int userId)
        {
            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterGameIdCamel, gameId),
                    new SqlParameter(ParameterUserIdCamel, userId)
                };

                var ownedGameDataTable = dataLink.ExecuteReader(StoredProcedure_GetOwnedGameById, sqlParameters);

                if (ownedGameDataTable.Rows.Count == 0)
                {
                    return null;
                }

                var ownedGame = MapDataRowToOwnedGame(ownedGameDataTable.Rows[0]);
                return ownedGame;
            }
            catch (SqlException sqlException)
            {
                throw new RepositoryException(Error_GetOwnedGameByIdDataBase, sqlException);
            }
            catch (Exception generalException)
            {
                throw new RepositoryException(Error_GetOwnedGameByIdUnexpected, generalException);
            }
        }

        public void RemoveOwnedGame(int gameId, int userId)
        {
            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter(ParameterGameIdUnderscore, gameId),
                    new SqlParameter(ParameterUserId, userId)
                };

                dataLink.ExecuteNonQuery(StoredProcedure_RemoveOwnedGame, sqlParameters);
            }
            catch (SqlException sqlException)
            {
                throw new RepositoryException(Error_RemoveOwnedGameDataBase, sqlException);
            }
            catch (Exception generalException)
            {
                throw new RepositoryException(Error_RemoveOwnedGameUnexpected, generalException);
            }
        }

        private static List<OwnedGame> MapDataTableToOwnedGames(DataTable dataTable)
        {
            var ownedGamesList = dataTable.AsEnumerable().Select(row =>
            {
                var ownedGame = new OwnedGame(
                    Convert.ToInt32(row[ColumnUserId]),
                    row[ColumnTitle].ToString(),
                    row[ColumnDescription]?.ToString(),
                    row[ColumnCoverPicture]?.ToString());

                ownedGame.GameId = Convert.ToInt32(row[ColumnGameId]);
                return ownedGame;
            }).ToList();

            return ownedGamesList;
        }

        private static OwnedGame MapDataRowToOwnedGame(DataRow dataRow)
        {
            var ownedGame = new OwnedGame(
                Convert.ToInt32(dataRow[ColumnUserId]),
                dataRow[ColumnTitle].ToString(),
                dataRow[ColumnDescription]?.ToString(),
                dataRow[ColumnCoverPicture]?.ToString());

            ownedGame.GameId = Convert.ToInt32(dataRow[ColumnGameId]);
            return ownedGame;
        }
    }
}
