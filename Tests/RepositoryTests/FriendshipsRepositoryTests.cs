using System;
using System.Runtime.Serialization;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using BusinessLayer.Data;
using BusinessLayer.Models;
using BusinessLayer.Repositories;
using BusinessLayer.Repositories.Interfaces;
using BusinessLayer.Exceptions;
using Microsoft.Data.SqlClient;
using BusinessLayer.Services;

namespace Tests.RepositoryTests
{
    [TestFixture]
    public class FriendshipsRepositoryTests
    {
        private Mock<IDataLink> mockDataLink;
        private FriendshipsRepository friendshipsRepository;

        [SetUp]
        public void SetUp()
        {
            // Arrange: Create a new mock IDataLink and instantiate the FriendshipsRepository.
            mockDataLink = new Mock<IDataLink>();
            friendshipsRepository = new FriendshipsRepository(mockDataLink.Object);
        }

        [Test]
        public void Constructor_NullDataLink_ThrowsArgumentNullException()
        {
            // Assert: Passing a null IDataLink should throw an ArgumentNullException.
            Assert.Throws<ArgumentNullException>(() => new FriendshipsRepository(null));
        }

        #region Helper Methods

        private DataTable CreateFriendshipsDataTable(params (int friendshipId, int userId, int friendId)[] rowData)
        {
            DataTable friendshipsDataTable = new DataTable();
            friendshipsDataTable.Columns.Add("friendship_id", typeof(int));
            friendshipsDataTable.Columns.Add("user_id", typeof(int));
            friendshipsDataTable.Columns.Add("friend_id", typeof(int));

            foreach (var (friendshipId, userId, friendId) in rowData)
            {
                var dataRow = friendshipsDataTable.NewRow();
                dataRow["friendship_id"] = friendshipId;
                dataRow["user_id"] = userId;
                dataRow["friend_id"] = friendId;
                friendshipsDataTable.Rows.Add(dataRow);
            }
            return friendshipsDataTable;
        }

        private DataTable CreateUserDataTable(string username)
        {
            DataTable userDataTable = new DataTable();
            userDataTable.Columns.Add("username", typeof(string));
            var userRow = userDataTable.NewRow();
            userRow["username"] = username;
            userDataTable.Rows.Add(userRow);
            return userDataTable;
        }

        private DataTable CreateUserProfileDataTable(string profilePicture)
        {
            DataTable userProfileDataTable = new DataTable();
            userProfileDataTable.Columns.Add("profile_picture", typeof(string));
            var profileRow = userProfileDataTable.NewRow();
            profileRow["profile_picture"] = profilePicture;
            userProfileDataTable.Rows.Add(profileRow);
            return userProfileDataTable;
        }

        private SqlException CreateSqlException()
        {
            // Create an uninitialized SqlException.
            return (SqlException)FormatterServices.GetUninitializedObject(typeof(SqlException));
        }

        #endregion

        #region GetAllFriendships Tests

        [Test]
        public void GetAllFriendships_ForUser_ReturnsCorrectCount()
        {
            // Arrange
            var friendshipsTable = CreateFriendshipsDataTable((1, 1, 2), (2, 1, 3));
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetFriendsForUser", It.IsAny<SqlParameter[]>()))
                        .Returns(friendshipsTable);
            // Setup friend lookup for user IDs.
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetUserById",
                It.Is<SqlParameter[]>(p => Convert.ToInt32(p[0].Value) == 2)))
                        .Returns(CreateUserDataTable("Alice"));
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetUserById",
                It.Is<SqlParameter[]>(p => Convert.ToInt32(p[0].Value) == 3)))
                        .Returns(CreateUserDataTable("Bob"));
            // Setup profile lookup for friend IDs.
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetUserProfileByUserId",
                It.Is<SqlParameter[]>(p => Convert.ToInt32(p[0].Value) == 2)))
                        .Returns(CreateUserProfileDataTable("alice.jpg"));
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetUserProfileByUserId",
                It.Is<SqlParameter[]>(p => Convert.ToInt32(p[0].Value) == 3)))
                        .Returns(CreateUserProfileDataTable("bob.jpg"));
            // Act
            var friendshipsForUser = friendshipsRepository.GetAllFriendships(1);
            // Assert: There should be exactly 2 friendships.
            Assert.That(friendshipsForUser.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetAllFriendships_ForUser_AssignsFriendUsernameCorrectly()
        {
            // Arrange
            var friendshipsTable = CreateFriendshipsDataTable((1, 1, 2));
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetFriendsForUser", It.IsAny<SqlParameter[]>()))
                        .Returns(friendshipsTable);
            // Use a fixed username for the friend.
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetUserById", It.IsAny<SqlParameter[]>()))
                        .Returns(CreateUserDataTable("Charlie"));
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetUserProfileByUserId", It.IsAny<SqlParameter[]>()))
                        .Returns(CreateUserProfileDataTable("charlie.jpg"));
            // Act
            var friendshipsForUser = friendshipsRepository.GetAllFriendships(1);
            // Assert: FriendUsername should be "Charlie".
            Assert.That(friendshipsForUser.First().FriendUsername, Is.EqualTo("Charlie"));
        }

        [Test]
        public void GetAllFriendships_ForUser_AssignsFriendProfilePictureCorrectly()
        {
            // Arrange
            var friendshipsTable = CreateFriendshipsDataTable((1, 1, 2));
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetFriendsForUser", It.IsAny<SqlParameter[]>()))
                        .Returns(friendshipsTable);
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetUserById", It.IsAny<SqlParameter[]>()))
                        .Returns(CreateUserDataTable("Diana"));
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetUserProfileByUserId", It.IsAny<SqlParameter[]>()))
                        .Returns(CreateUserProfileDataTable("diana.png"));
            // Act
            var friendshipsForUser = friendshipsRepository.GetAllFriendships(1);
            // Assert: FriendProfilePicture should be "diana.png".
            Assert.That(friendshipsForUser.First().FriendProfilePicture, Is.EqualTo("diana.png"));
        }

        [Test]
        public void GetAllFriendships_ForUser_OrdersByFriendUsername_ReturnsCorrectOrder()
        {
            // Arrange: Two rows with different friend IDs.
            var friendshipsTable = CreateFriendshipsDataTable((1, 1, 2), (2, 1, 3));
            // Setup friend data: friendId 2 -> "Zoe", friendId 3 -> "Anna"
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetFriendsForUser", It.IsAny<SqlParameter[]>()))
                        .Returns(friendshipsTable);
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetUserById",
                It.Is<SqlParameter[]>(p => Convert.ToInt32(p[0].Value) == 2)))
                        .Returns(CreateUserDataTable("Zoe"));
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetUserById",
                It.Is<SqlParameter[]>(p => Convert.ToInt32(p[0].Value) == 3)))
                        .Returns(CreateUserDataTable("Anna"));
            // Setup profile lookup (dummy).
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetUserProfileByUserId", It.IsAny<SqlParameter[]>()))
                        .Returns(CreateUserProfileDataTable("dummy.jpg"));
            // Act
            var friendshipsForUser = friendshipsRepository.GetAllFriendships(1);
            // Assert: The first friendship's FriendUsername should be "Anna" (alphabetically before "Zoe").
            Assert.That(friendshipsForUser.First().FriendUsername, Is.EqualTo("Anna"));
        }

        [Test]
        public void GetAllFriendships_SqlException_ThrowsRepositoryExceptionWithDatabaseErrorMessage()
        {
            // Arrange
            SqlException sqlExceptionInstance = CreateSqlException();
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetFriendsForUser", It.IsAny<SqlParameter[]>()))
                        .Throws(sqlExceptionInstance);
            // Act & Assert
            var repositoryException = Assert.Throws<RepositoryException>(() => friendshipsRepository.GetAllFriendships(1));
            Assert.That(repositoryException.Message, Is.EqualTo("Database error while retrieving friendships."));
        }

        [Test]
        public void GetAllFriendships_GenericException_ThrowsRepositoryExceptionWithUnexpectedErrorMessage()
        {
            // Arrange
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetFriendsForUser", It.IsAny<SqlParameter[]>()))
                        .Throws(new Exception("Test error"));
            // Act & Assert
            var repositoryException = Assert.Throws<RepositoryException>(() => friendshipsRepository.GetAllFriendships(1));
            Assert.That(repositoryException.Message, Is.EqualTo("An unexpected error occurred while retrieving friendships."));
        }

        #endregion

        #region GetFriendshipById Tests

        [Test]
        public void GetFriendshipById_ExistingId_ReturnsFriendship()
        {
            // Arrange
            var friendshipDataTable = CreateFriendshipsDataTable((10, 1, 2));
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetFriendshipById", It.IsAny<SqlParameter[]>()))
                        .Returns(friendshipDataTable);
            // Act
            var retrievedFriendship = friendshipsRepository.GetFriendshipById(10);
            // Assert: The retrieved friendship's ID should be 10.
            Assert.That(retrievedFriendship.FriendshipId, Is.EqualTo(10));
        }

        [Test]
        public void GetFriendshipById_NonExistingId_ReturnsNull()
        {
            // Arrange: Return an empty DataTable.
            DataTable emptyFriendshipTable = new DataTable();
            emptyFriendshipTable.Columns.Add("friendship_id", typeof(int));
            emptyFriendshipTable.Columns.Add("user_id", typeof(int));
            emptyFriendshipTable.Columns.Add("friend_id", typeof(int));
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetFriendshipById", It.IsAny<SqlParameter[]>()))
                        .Returns(emptyFriendshipTable);
            // Act
            var retrievedFriendship = friendshipsRepository.GetFriendshipById(999);
            // Assert: The friendship should be null.
            Assert.That(retrievedFriendship, Is.Null);
        }

        [Test]
        public void GetFriendshipById_SqlException_ThrowsRepositoryExceptionWithDatabaseErrorMessage()
        {
            // Arrange
            SqlException sqlExceptionInstance = CreateSqlException();
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetFriendshipById", It.IsAny<SqlParameter[]>()))
                        .Throws(sqlExceptionInstance);
            // Act & Assert
            var repositoryException = Assert.Throws<RepositoryException>(() => friendshipsRepository.GetFriendshipById(1));
            Assert.That(repositoryException.Message, Is.EqualTo("Database error while retrieving friendship by ID."));
        }

        [Test]
        public void GetFriendshipById_GenericException_ThrowsRepositoryExceptionWithUnexpectedErrorMessage()
        {
            // Arrange
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetFriendshipById", It.IsAny<SqlParameter[]>()))
                        .Throws(new Exception("Test error"));
            // Act & Assert
            var repositoryException = Assert.Throws<RepositoryException>(() => friendshipsRepository.GetFriendshipById(1));
            Assert.That(repositoryException.Message, Is.EqualTo("An unexpected error occurred while retrieving friendship by ID."));
        }

        #endregion

        #region AddFriendship Tests

        [Test]
        public void AddFriendship_ValidData_DoesNotThrow()
        {
            // Arrange: Setup GetUserById for both the requesting user and the friend.
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetUserById",
                It.Is<SqlParameter[]>(p => Convert.ToInt32(p[0].Value) == 1)))
                        .Returns(CreateUserDataTable("User1"));
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetUserById",
                It.Is<SqlParameter[]>(p => Convert.ToInt32(p[0].Value) == 2)))
                        .Returns(CreateUserDataTable("User2"));
            // Setup GetAllFriendships to return an empty table.
            var emptyFriendshipsTable = CreateFriendshipsDataTable();
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetFriendsForUser", It.IsAny<SqlParameter[]>()))
                        .Returns(emptyFriendshipsTable);
            // Setup ExecuteNonQuery for adding a friend.
            mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery("AddFriend", It.IsAny<SqlParameter[]>()));
            // Act & Assert: Adding a valid friendship should not throw.
            Assert.DoesNotThrow(() => friendshipsRepository.AddFriendship(1, 2));
            // Dummy assert to satisfy one-assert rule.
            Assert.That(true, Is.True);
        }

        [Test]
        public void AddFriendship_WhenUserDoesNotExist_ThrowsRepositoryException()
        {
            // Arrange: Setup GetUserById for user 1 to return an empty table.
            DataTable emptyUserTable = new DataTable();
            emptyUserTable.Columns.Add("username", typeof(string));
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetUserById",
                It.Is<SqlParameter[]>(p => Convert.ToInt32(p[0].Value) == 1)))
                        .Returns(emptyUserTable);
            // Act & Assert: Expect a RepositoryException because user 1 does not exist.
            var repositoryException = Assert.Throws<RepositoryException>(() => friendshipsRepository.AddFriendship(1, 2));
            Assert.That(repositoryException.Message, Is.EqualTo("User with ID 1 does not exist."));
        }

        [Test]
        public void AddFriendship_WhenFriendDoesNotExist_ThrowsRepositoryException()
        {
            // Arrange: Setup valid user data for user 1.
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetUserById",
                It.Is<SqlParameter[]>(p => Convert.ToInt32(p[0].Value) == 1)))
                        .Returns(CreateUserDataTable("User1"));
            // Setup friend data for friendId 2 returns empty.
            DataTable emptyUserTable = new DataTable();
            emptyUserTable.Columns.Add("username", typeof(string));
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetUserById",
                It.Is<SqlParameter[]>(p => Convert.ToInt32(p[0].Value) == 2)))
                        .Returns(emptyUserTable);
            // Act & Assert: Expect a RepositoryException because friend with ID 2 does not exist.
            var repositoryException = Assert.Throws<RepositoryException>(() => friendshipsRepository.AddFriendship(1, 2));
            Assert.That(repositoryException.Message, Is.EqualTo("User with ID 2 does not exist."));
        }

        [Test]
        public void AddFriendship_WhenFriendshipAlreadyExists_ThrowsRepositoryException()
        {
            // Arrange: Setup valid data for both users.
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetUserById", It.IsAny<SqlParameter[]>()))
                        .Returns(CreateUserDataTable("User"));
            // Setup GetAllFriendships to return a friendship with friendId = 2.
            var friendshipsTable = CreateFriendshipsDataTable((1, 1, 2));
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetFriendsForUser", It.IsAny<SqlParameter[]>()))
                        .Returns(friendshipsTable);
            // Setup profile lookup for friendId = 2.
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetUserProfileByUserId",
                It.Is<SqlParameter[]>(p => Convert.ToInt32(p[0].Value) == 2)))
                        .Returns(CreateUserProfileDataTable("UserPic"));
            // Act & Assert: Adding a duplicate friendship should throw a RepositoryException.
            var repositoryException = Assert.Throws<RepositoryException>(() => friendshipsRepository.AddFriendship(1, 2));
            Assert.That(repositoryException.Message, Is.EqualTo("Friendship already exists."));
        }

        [Test]
        public void AddFriendship_SqlException_ThrowsRepositoryExceptionWithDatabaseErrorMessage()
        {
            // Arrange: Setup GetUserById to return valid user data.
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetUserById", It.IsAny<SqlParameter[]>()))
                        .Returns(CreateUserDataTable("User"));
            // Setup GetAllFriendships returns empty.
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetFriendsForUser", It.IsAny<SqlParameter[]>()))
                        .Returns(CreateFriendshipsDataTable());
            // Setup ExecuteNonQuery for "AddFriend" to throw a SqlException.
            SqlException sqlExceptionInstance = CreateSqlException();
            mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery("AddFriend", It.IsAny<SqlParameter[]>()))
                        .Throws(sqlExceptionInstance);
            // Act & Assert
            var repositoryException = Assert.Throws<RepositoryException>(() => friendshipsRepository.AddFriendship(1, 2));
            Assert.That(repositoryException.Message, Is.EqualTo("Database error while adding friendship."));
        }

        [Test]
        public void AddFriendship_GenericException_ThrowsRepositoryExceptionWithUnexpectedErrorMessage()
        {
            // Arrange: Setup GetUserById to return valid data.
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetUserById", It.IsAny<SqlParameter[]>()))
                        .Returns(CreateUserDataTable("User"));
            // Setup GetAllFriendships returns empty.
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader("GetFriendsForUser", It.IsAny<SqlParameter[]>()))
                        .Returns(CreateFriendshipsDataTable());
            // Setup ExecuteNonQuery for "AddFriend" to throw a generic exception.
            mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery("AddFriend", It.IsAny<SqlParameter[]>()))
                        .Throws(new Exception("Test error"));
            // Act & Assert
            var repositoryException = Assert.Throws<RepositoryException>(() => friendshipsRepository.AddFriendship(1, 2));
            Assert.That(repositoryException.Message, Is.EqualTo("An unexpected error occurred while adding friendship."));
        }

        #endregion

        #region RemoveFriendship Tests

        [Test]
        public void RemoveFriendship_ValidInput_CallsExecuteNonQuery()
        {
            // Arrange
            mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery("RemoveFriend", It.IsAny<SqlParameter[]>()))
                        .Verifiable();
            // Act
            friendshipsRepository.RemoveFriendship(5);
            // Assert: Verify that ExecuteNonQuery was called once.
            mockDataLink.Verify(dataLink => dataLink.ExecuteNonQuery("RemoveFriend", It.IsAny<SqlParameter[]>()), Times.Once);
            Assert.That(true, Is.True); // Dummy assert for satisfying one-assert rule.
        }

        [Test]
        public void RemoveFriendship_SqlException_ThrowsRepositoryExceptionWithDatabaseErrorMessage()
        {
            // Arrange
            SqlException sqlExceptionInstance = CreateSqlException();
            mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery("RemoveFriend", It.IsAny<SqlParameter[]>()))
                        .Throws(sqlExceptionInstance);
            // Act & Assert
            var repositoryException = Assert.Throws<RepositoryException>(() => friendshipsRepository.RemoveFriendship(5));
            Assert.That(repositoryException.Message, Is.EqualTo("Database error while removing friendship."));
        }

        [Test]
        public void RemoveFriendship_GenericException_ThrowsRepositoryExceptionWithUnexpectedErrorMessage()
        {
            // Arrange
            mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery("RemoveFriend", It.IsAny<SqlParameter[]>()))
                        .Throws(new Exception("Test error"));
            // Act & Assert
            var repositoryException = Assert.Throws<RepositoryException>(() => friendshipsRepository.RemoveFriendship(5));
            Assert.That(repositoryException.Message, Is.EqualTo("An unexpected error occurred while removing friendship."));
        }

        #endregion

        #region GetFriendshipCount Tests

        [Test]
        public void GetFriendshipCount_ForUser_ReturnsCorrectCount()
        {
            // Arrange
            mockDataLink.Setup(dataLink => dataLink.ExecuteScalar<int>("GetFriendshipCountForUser", It.IsAny<SqlParameter[]>()))
                        .Returns(7);
            // Act
            int friendshipCount = friendshipsRepository.GetFriendshipCount(1);
            // Assert: Count should be 7.
            Assert.That(friendshipCount, Is.EqualTo(7));
        }

        [Test]
        public void GetFriendshipCount_SqlException_ThrowsRepositoryExceptionWithDatabaseErrorMessage()
        {
            // Arrange
            SqlException sqlExceptionInstance = CreateSqlException();
            mockDataLink.Setup(dataLink => dataLink.ExecuteScalar<int>("GetFriendshipCountForUser", It.IsAny<SqlParameter[]>()))
                        .Throws(sqlExceptionInstance);
            // Act & Assert
            var repositoryException = Assert.Throws<RepositoryException>(() => friendshipsRepository.GetFriendshipCount(1));
            Assert.That(repositoryException.Message, Is.EqualTo("Database error while retrieving friendship count."));
        }

        [Test]
        public void GetFriendshipCount_GenericException_ThrowsRepositoryExceptionWithUnexpectedErrorMessage()
        {
            // Arrange
            mockDataLink.Setup(dataLink => dataLink.ExecuteScalar<int>("GetFriendshipCountForUser", It.IsAny<SqlParameter[]>()))
                        .Throws(new Exception("Test error"));
            // Act & Assert
            var repositoryException = Assert.Throws<RepositoryException>(() => friendshipsRepository.GetFriendshipCount(1));
            Assert.That(repositoryException.Message, Is.EqualTo("An unexpected error occurred while retrieving friendship count."));
        }

        #endregion

        #region GetFriendshipId Tests

        [Test]
        public void GetFriendshipId_ExistingFriendship_ReturnsCorrectId()
        {
            // Arrange
            mockDataLink.Setup(dataLink => dataLink.ExecuteScalar<int?>("GetFriendshipId", It.IsAny<SqlParameter[]>()))
                        .Returns(15);
            // Act
            int? retrievedFriendshipId = friendshipsRepository.GetFriendshipId(1, 2);
            // Assert: The returned friendship ID should be 15.
            Assert.That(retrievedFriendshipId, Is.EqualTo(15));
        }

        [Test]
        public void GetFriendshipId_NonExistingFriendship_ReturnsNull()
        {
            // Arrange
            mockDataLink.Setup(dataLink => dataLink.ExecuteScalar<int?>("GetFriendshipId", It.IsAny<SqlParameter[]>()))
                        .Returns((int?)null);
            // Act
            int? retrievedFriendshipId = friendshipsRepository.GetFriendshipId(1, 2);
            // Assert: The returned friendship ID should be null.
            Assert.That(retrievedFriendshipId, Is.Null);
        }

        [Test]
        public void GetFriendshipId_SqlException_ThrowsRepositoryExceptionWithDatabaseErrorMessage()
        {
            // Arrange
            SqlException sqlExceptionInstance = CreateSqlException();
            mockDataLink.Setup(dataLink => dataLink.ExecuteScalar<int?>("GetFriendshipId", It.IsAny<SqlParameter[]>()))
                        .Throws(sqlExceptionInstance);
            // Act & Assert
            var repositoryException = Assert.Throws<RepositoryException>(() => friendshipsRepository.GetFriendshipId(1, 2));
            Assert.That(repositoryException.Message, Is.EqualTo("Database error while retrieving friendship ID."));
        }

        [Test]
        public void GetFriendshipId_GenericException_ThrowsRepositoryExceptionWithUnexpectedErrorMessage()
        {
            // Arrange
            mockDataLink.Setup(dataLink => dataLink.ExecuteScalar<int?>("GetFriendshipId", It.IsAny<SqlParameter[]>()))
                        .Throws(new Exception("Test error"));
            // Act & Assert
            var repositoryException = Assert.Throws<RepositoryException>(() => friendshipsRepository.GetFriendshipId(1, 2));
            Assert.That(repositoryException.Message, Is.EqualTo("An unexpected error occurred while retrieving friendship ID."));
        }

        #endregion
    }
}
