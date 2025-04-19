using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using BusinessLayer.Data;
using BusinessLayer.Repositories;
using BusinessLayer.Repositories.Interfaces;
using BusinessLayer.Models;
using BusinessLayer.Exceptions;
using Moq;
using Microsoft.Data.SqlClient;

namespace Tests.RepositoryTests
{
    [TestFixture]
    internal class UserRepositoryTests
    {
        private UsersRepository usersRepository;
        private Mock<IDataLink> dataLinkMock;
        private DataTable dataTable;

        [SetUp]
        public void SetUp()
        {
            dataLinkMock = new Mock<IDataLink>();
            dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("username", typeof(string));
            dataTable.Columns.Add("email", typeof(string));
            dataTable.Columns.Add("developer", typeof(bool));
            dataTable.Columns.Add("created_at", typeof(DateTime));
            dataTable.Columns.Add("last_login", typeof(DateTime));

            dataTable.Rows.Add(1, "user1", "user1@example.com", true, DateTime.Now, DateTime.Now.AddDays(-1));
            dataTable.Rows.Add(2, "user2", "user2@example.com", false, DateTime.Now, DBNull.Value);

            usersRepository = new UsersRepository(dataLinkMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            // Clear rows in the DataTable to ensure no state is carried over between tests
            dataTable.Rows.Clear();

            // If necessary, you can explicitly set it to null or handle other cleanup here
            dataTable?.Dispose();
            dataTable = null;
        }

        [Test]
        public void GetAllUsers_RetrievesUsersCorrectly_ReturnsUsers()
        {
            // Arrange
            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader("GetAllUsers", It.IsAny<SqlParameter[]>())).Returns(dataTable);

            // Act
            List<User> users = usersRepository.GetAllUsers();

            // Assert
            Assert.That(users.Count(), Is.EqualTo(2));
        }

        [Test]
        public void GetAllUsers_SqlFails_ThrowsRepositoryException()
        {
            // Arrange
            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                .Throws(new DatabaseOperationException("Database error"));

            // Act & Assert
            Assert.Throws<RepositoryException>(() => usersRepository.GetAllUsers());
        }

        [Test]
        public void GetUserById_UserExists_ReturnUser()
        {
            // Arrange
            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader("GetUserById", It.IsAny<SqlParameter[]>())).Returns(dataTable);

            // Act
            User user = usersRepository.GetUserById(1);

            // Assert
            Assert.That(user, Is.Not.Null);
        }

        [Test]
        public void GetUserById_UserDoesntExists_ReturnNull()
        {
            // Arrange
            var emptyDataTable = new DataTable();
            emptyDataTable.Columns.Add("user_id", typeof(int));
            emptyDataTable.Columns.Add("username", typeof(string));
            emptyDataTable.Columns.Add("email", typeof(string));
            emptyDataTable.Columns.Add("developer", typeof(bool));
            emptyDataTable.Columns.Add("created_at", typeof(DateTime));
            emptyDataTable.Columns.Add("last_login", typeof(DateTime));
            emptyDataTable.Columns.Add("password", typeof(string));

            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader("GetUserById", It.IsAny<SqlParameter[]>())).Returns(emptyDataTable);

            // Act
            User user = usersRepository.GetUserById(3);

            // Assert
            Assert.That(user, Is.Null);
        }

        [Test]
        public void GetUserById_SqlException_ThrowRepositoryException()
        {
            // Arrange
            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                .Throws(new DatabaseOperationException("Database error"));

            // Act & Assert
            Assert.Throws<RepositoryException>(() => usersRepository.GetUserById(10));
        }

        [Test]
        public void UpdateUser_CorrectUser_ReturnsUpdatedUser()
        {
            // Arrange
            var updatedUser = new User
            {
                UserId = 1, // Assuming UserId is required for the update
                Username = "user10",
                Email = "user10@example.com",
                Password = "somepassword123", // Example password
                IsDeveloper = true,
                CreatedAt = DateTime.Now,
                LastLogin = DateTime.Now.AddDays(-1)
            };

            // Mock the ExecuteReader method to return the updated user data
            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("username", typeof(string));
            dataTable.Columns.Add("email", typeof(string));
            dataTable.Columns.Add("developer", typeof(bool));
            dataTable.Columns.Add("created_at", typeof(DateTime));
            dataTable.Columns.Add("last_login", typeof(DateTime));

            dataTable.Rows.Add(1, "user10", "user10@example.com", true, DateTime.Now, DateTime.Now.AddDays(-1));

            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader("UpdateUser", It.IsAny<SqlParameter[]>())).Returns(dataTable);

            // Act
            User user = usersRepository.UpdateUser(updatedUser);

            // Assert
            Assert.That(user.Username, Is.EqualTo("user10"));
        }

        [Test]
        public void UpdateUser_NonExistantUser_ThrowsRepositoryException()
        {
            // Arrange
            var updatedUser = new User
            {
                UserId = 1,
                Username = "user10",
                Email = "user10@example.com",
                Password = "somepassword123",
                IsDeveloper = true,
                CreatedAt = DateTime.Now,
                LastLogin = DateTime.Now.AddDays(-1)
            };

            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("username", typeof(string));
            dataTable.Columns.Add("email", typeof(string));
            dataTable.Columns.Add("developer", typeof(bool));
            dataTable.Columns.Add("created_at", typeof(DateTime));
            dataTable.Columns.Add("last_login", typeof(DateTime));

            // No rows added to simulate no user found
            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader("UpdateUser", It.IsAny<SqlParameter[]>())).Returns(dataTable);

            // Act and Assert
            Assert.Throws<RepositoryException>(() => usersRepository.UpdateUser(updatedUser));
        }

        [Test]
        public void UpdateUser_SqlError_ThrowsRepositoryException()
        {
            // Arrange
            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                .Throws(new DatabaseOperationException("Database error"));

            // Act & Assert
            Assert.Throws<RepositoryException>(() => usersRepository.UpdateUser(new User()));
        }

        [Test]
        public void CreateUser_CorrectUser_UserIsAdded()
        {
            // Arrange
            var newUser = new User
            {
                UserId = 1,
                Username = "user10",
                Email = "user10@example.com",
                Password = "somepassword123",
                IsDeveloper = true,
                CreatedAt = DateTime.Now,
                LastLogin = DateTime.Now.AddDays(-1)
            };

            // Create a DataTable with a row that simulates the successful creation of the user
            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("username", typeof(string));
            dataTable.Columns.Add("email", typeof(string));
            dataTable.Columns.Add("developer", typeof(bool));
            dataTable.Columns.Add("created_at", typeof(DateTime));
            dataTable.Columns.Add("last_login", typeof(DateTime));

            // Add a row to the dataTable that simulates the created user being returned by the database
            dataTable.Rows.Add(newUser.UserId, newUser.Username, newUser.Email, newUser.IsDeveloper, newUser.CreatedAt, newUser.LastLogin);

            // Mock ExecuteReader to return the populated dataTable
            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader("CreateUser", It.IsAny<SqlParameter[]>())).Returns(dataTable);

            // Act
            User user = usersRepository.CreateUser(newUser);

            // Assert
            Assert.That(dataTable.Rows.Count, Is.EqualTo(1));  // Verify that the row is added
            Assert.That(user.Username, Is.EqualTo(newUser.Username));  // Verify that the returned user has the correct username
            Assert.That(user.Email, Is.EqualTo(newUser.Email));  // Verify the email
        }

        [Test]
        public void CreateUser_AddFails_ThrowsRepositoryException()
        {
            // Arrange
            var newUser = new User
            {
                UserId = 1,
                Username = "user10",
                Email = "user10@example.com",
                Password = "somepassword123",
                IsDeveloper = true,
                CreatedAt = DateTime.Now,
                LastLogin = DateTime.Now.AddDays(-1)
            };

            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("username", typeof(string));
            dataTable.Columns.Add("email", typeof(string));
            dataTable.Columns.Add("developer", typeof(bool));
            dataTable.Columns.Add("created_at", typeof(DateTime));
            dataTable.Columns.Add("last_login", typeof(DateTime));

            // No rows added to simulate no user found
            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader("CreateUser", It.IsAny<SqlParameter[]>())).Returns(dataTable);

            // Act and Assert
            Assert.Throws<RepositoryException>(() => usersRepository.CreateUser(newUser));
        }

        [Test]
        public void CreateUser_SqlError_ThrowsRepositoryException()
        {
            // Arrange
            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                .Throws(new DatabaseOperationException("Database error"));

            // Act & Assert
            Assert.Throws<RepositoryException>(() => usersRepository.CreateUser(new User()));
        }

        [Test]
        public void DeleteUser_ValidUser_UserIsDeleted()
        {
            // Arrange
            int userIdToDelete = 1; // The ID of the user we want to delete
            dataLinkMock.Setup(dataLink => dataLink.ExecuteNonQuery("DeleteUser", It.IsAny<SqlParameter[]>())).Returns(1); // Simulate successful deletion (1 row affected)

            // Act
            usersRepository.DeleteUser(userIdToDelete);

            // Assert
            dataLinkMock.Verify(dataLink => dataLink.ExecuteNonQuery("DeleteUser", It.Is<SqlParameter[]>(p => (int)p[0].Value == userIdToDelete)), Times.Once);
        }

        [Test]
        public void DeleteUser_SqlError_ThrowsRepositoryException()
        {
            // Arrange
            dataLinkMock.Setup(dataLink => dataLink.ExecuteNonQuery(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                .Throws(new DatabaseOperationException("Database error"));

            // Act & Assert
            Assert.Throws<RepositoryException>(() => usersRepository.DeleteUser(1));
        }

        [Test]
        public void VerifyCredentials_ValidCredentials_ReturnsUser()
        {
            // Arrange
            var emailOrUsername = "user10@example.com"; // A valid email or username for the test user
            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("username", typeof(string));
            dataTable.Columns.Add("hashed_password", typeof(string)); // Correct column name used
            dataTable.Columns.Add("developer", typeof(bool));
            dataTable.Columns.Add("created_at", typeof(DateTime));
            dataTable.Columns.Add("last_login", typeof(DateTime));
            dataTable.Columns.Add("email", typeof(string)); // Correct column name used

            // Simulate a row for the valid user
            dataTable.Rows.Add(1, "user10", "hashedpassword123", true, DateTime.Now, DateTime.Now.AddDays(-1), "user10@example.com");

            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader("GetUserByEmailOrUsername", It.IsAny<SqlParameter[]>())).Returns(dataTable);

            // Act
            User? user = usersRepository.VerifyCredentials(emailOrUsername);

            // Assert
            Assert.That(user?.Email, Is.EqualTo("user10@example.com")); // Ensure that the returned user's email matches
        }

        [Test]
        public void VerifyCredentials_InvalidCredentials_ReturnsNull()
        {
            // Arrange
            var emailOrUsername = "nonexistentuser@example.com"; // An email or username that doesn't exist
            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("username", typeof(string));
            dataTable.Columns.Add("email", typeof(string));
            dataTable.Columns.Add("developer", typeof(bool));
            dataTable.Columns.Add("created_at", typeof(DateTime));
            dataTable.Columns.Add("last_login", typeof(DateTime));
            dataTable.Columns.Add("password", typeof(string)); // Assuming password column exists

            // No rows added to simulate no user found
            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader("GetUserByEmailOrUsername", It.IsAny<SqlParameter[]>())).Returns(dataTable);

            // Act
            User? user = usersRepository.VerifyCredentials(emailOrUsername);

            // Assert
            Assert.That(user, Is.Null); // Assert that null is returned
        }

        [Test]
        public void VerifyCredentials_SqlError_ThrowsRepositoryException()
        {
            // Arrange
            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                .Throws(new DatabaseOperationException("Database error"));

            // Act & Assert
            Assert.Throws<RepositoryException>(() => usersRepository.VerifyCredentials("irrelevant"));
        }

        [Test]
        public void GetUserByEmail_CorrectEmail_GetsUser()
        {
            // Arrange
            var email = "user10@example.com"; // A valid email or username for the test user
            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("username", typeof(string));
            dataTable.Columns.Add("email", typeof(string));
            dataTable.Columns.Add("developer", typeof(bool));
            dataTable.Columns.Add("created_at", typeof(DateTime));
            dataTable.Columns.Add("last_login", typeof(DateTime));
            dataTable.Columns.Add("password", typeof(string)); // Assuming password column exists

            // Simulate a row for the valid user
            dataTable.Rows.Add(1, "user10", "user10@example.com", true, DateTime.Now, DateTime.Now.AddDays(-1), "hashedpassword123");

            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader("GetUserByEmail", It.IsAny<SqlParameter[]>())).Returns(dataTable);

            // Act
            User? user = usersRepository.GetUserByEmail(email);

            // Assert
            Assert.That(user?.Email, Is.EqualTo("user10@example.com")); // Ensure that the returned user's email matches
        }

        [Test]
        public void GetUserByEmail_WrongEmail_GetsNull()
        {
            // Arrange
            var email = "worng102321@example.com"; // A valid email or username for the test user
            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("username", typeof(string));
            dataTable.Columns.Add("email", typeof(string));
            dataTable.Columns.Add("developer", typeof(bool));
            dataTable.Columns.Add("created_at", typeof(DateTime));
            dataTable.Columns.Add("last_login", typeof(DateTime));
            dataTable.Columns.Add("password", typeof(string)); // Assuming password column exists

            // Simulate no row for the valid user
            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader("GetUserByEmail", It.IsAny<SqlParameter[]>())).Returns(dataTable);

            // Act
            User? user = usersRepository.GetUserByEmail(email);

            // Assert
            Assert.That(user, Is.Null);
        }

        [Test]
        public void GetUserByEmail_SqlError_ThrowsRepositoryException()
        {
            // Arrange
            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                .Throws(new DatabaseOperationException("Database error"));

            // Act & Assert
            Assert.Throws<RepositoryException>(() => usersRepository.GetUserByEmail("irrelevant"));
        }

        [Test]
        public void GetUserByUsername_CorrectUsername_GetsUser()
        {
            // Arrange
            var username = "username"; // A valid email or username for the test user
            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("username", typeof(string));
            dataTable.Columns.Add("email", typeof(string));
            dataTable.Columns.Add("developer", typeof(bool));
            dataTable.Columns.Add("created_at", typeof(DateTime));
            dataTable.Columns.Add("last_login", typeof(DateTime));
            dataTable.Columns.Add("password", typeof(string)); // Assuming password column exists

            // Simulate a row for the valid user
            dataTable.Rows.Add(1, "user10", "user10@example.com", true, DateTime.Now, DateTime.Now.AddDays(-1), "hashedpassword123");

            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader("GetUserByUsername", It.IsAny<SqlParameter[]>())).Returns(dataTable);

            // Act
            User? user = usersRepository.GetUserByUsername(username);

            // Assert
            Assert.That(user?.Email, Is.EqualTo("user10@example.com")); // Ensure that the returned user's email matches
        }

        [Test]
        public void GetUserByUsername_WrongUsername_GetsNull()
        {
            // Arrange
            var username = "username";
            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("username", typeof(string));
            dataTable.Columns.Add("email", typeof(string));
            dataTable.Columns.Add("developer", typeof(bool));
            dataTable.Columns.Add("created_at", typeof(DateTime));
            dataTable.Columns.Add("last_login", typeof(DateTime));
            dataTable.Columns.Add("password", typeof(string)); // Assuming password column exists

            // Simulate no row for the valid user
            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader("GetUserByUsername", It.IsAny<SqlParameter[]>())).Returns(dataTable);

            // Act
            User? user = usersRepository.GetUserByUsername(username);

            // Assert
            Assert.That(user, Is.Null);
        }

        [Test]
        public void GetUserByUsername_SqlError_ThrowsRepositoryException()
        {
            // Arrange
            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                .Throws(new DatabaseOperationException("Database error"));

            // Act & Assert
            Assert.Throws<RepositoryException>(() => usersRepository.GetUserByUsername("irrelevant"));
        }

        [Test]
        public void CheckUserExists_UserExistsErrorTypeExists_ReturnsErrorType()
        {
            // Arrange
            var email = "user10@example.com";
            var username = "user10";

            var dataTable = new DataTable();
            dataTable.Columns.Add("ErrorType", typeof(string));

            // Simulate a user exists and an error type is returned
            dataTable.Rows.Add("Email or Username already taken");

            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader("CheckUserExists", It.IsAny<SqlParameter[]>())).Returns(dataTable);

            // Act
            string errorType = usersRepository.CheckUserExists(email, username);

            // Assert
            Assert.That(errorType, Is.EqualTo("Email or Username already taken"));
        }

        [Test]
        public void CheckUserExists_ErrorTypeIsNull_ReturnsNull()
        {
            // Arrange
            var email = "user10@example.com";
            var username = "user10";

            var dataTable = new DataTable();
            dataTable.Columns.Add("ErrorType", typeof(string));

            // Simulate a row where ErrorType is null (DBNull.Value)
            dataTable.Rows.Add(DBNull.Value);

            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader("CheckUserExists", It.IsAny<SqlParameter[]>())).Returns(dataTable);

            // Act
            string? errorType = usersRepository.CheckUserExists(email, username);

            // Assert
            Assert.That(errorType, Is.Null); // Since ErrorType is null (DBNull), the return value should also be null
        }

        [Test]
        public void CheckUserExists_UserDoesNotExist_ReturnsNull()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var username = "nonexistent";

            var dataTable = new DataTable();
            dataTable.Columns.Add("ErrorType", typeof(string));

            // Simulate no user found, so no rows are returned
            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader("CheckUserExists", It.IsAny<SqlParameter[]>())).Returns(dataTable);

            // Act
            string errorType = usersRepository.CheckUserExists(email, username);

            // Assert
            Assert.That(errorType, Is.Null); // Assert that no error type is returned for a nonexistent user
        }

        [Test]
        public void CheckUserExists_SqlError_ThrowsRepositoryException()
        {
            // Arrange
            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                .Throws(new DatabaseOperationException("Database error"));

            // Act & Assert
            Assert.Throws<RepositoryException>(() => usersRepository.CheckUserExists("irrelevant", "also"));
        }

        [Test]
        public void ChangeEmail_Called_Returns()
        {
            // Arrange
            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader("ChangeEmail", It.IsAny<SqlParameter[]>())).Verifiable();

            // Act and Assert
            Assert.DoesNotThrow(() => usersRepository.ChangeEmail(1, "new@mail.com"));
        }

        [Test]
        public void ChangeEmail_SqlError_ThrowsRepositoryException()
        {
            // Arrange
            dataLinkMock.Setup(dataLink => dataLink.ExecuteNonQuery(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                .Throws(new DatabaseOperationException("Database error"));

            // Act & Assert
            Assert.Throws<RepositoryException>(() => usersRepository.ChangeEmail(1, "also"));
        }

        [Test]
        public void ChangePassword_Called_Returns()
        {
            // Arrange
            dataLinkMock.Setup(dataLink => dataLink.ExecuteReader("ChangePassword", It.IsAny<SqlParameter[]>())).Verifiable();

            // Act and Assert
            Assert.DoesNotThrow(() => usersRepository.ChangePassword(1, "newPassword"));
        }

        [Test]
        public void ChangePassword_SqlError_ThrowsRepositoryException()
        {
            // Arrange
            dataLinkMock.Setup(dataLink => dataLink.ExecuteNonQuery(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                .Throws(new DatabaseOperationException("Database error"));

            // Act & Assert
            Assert.Throws<RepositoryException>(() => usersRepository.ChangePassword(1, "also"));
        }

        [Test]
        public void ChangeUsername_Called_Returns()
        {
            // Arrange
            dataLinkMock.Setup(dataLink => dataLink.ExecuteNonQuery("ChangeUsername", It.IsAny<SqlParameter[]>())).Verifiable();

            // Act and Assert
            Assert.DoesNotThrow(() => usersRepository.ChangeUsername(1, "newUsername"));
        }

        [Test]
        public void ChangeUsername_SqlError_ThrowsRepositoryException()
        {
            // Arrange
            dataLinkMock.Setup(dataLink => dataLink.ExecuteNonQuery(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                .Throws(new DatabaseOperationException("Database error"));

            // Act & Assert
            Assert.Throws<RepositoryException>(() => usersRepository.ChangeUsername(1, "also"));
        }

        [Test]
        public void UpdateLastLogin_Called_Returns()
        {
            // Arrange
            dataLinkMock.Setup(dataLink => dataLink.ExecuteNonQuery("UpdateLastLogin", It.IsAny<SqlParameter[]>())).Verifiable();

            // Act and Assert
            Assert.DoesNotThrow(() => usersRepository.UpdateLastLogin(1));
        }

        [Test]
        public void UpdateLastLogin_SqlError_ThrowsRepositoryException()
        {
            // Arrange
            dataLinkMock.Setup(dataLink => dataLink.ExecuteNonQuery(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                .Throws(new DatabaseOperationException("Database error"));

            // Act & Assert
            Assert.Throws<RepositoryException>(() => usersRepository.UpdateLastLogin(1));
        }

        [Test]
        public void MapDataRowToUser_ValidDataRow_ReturnsUser()
        {
            // Arrange
            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("username", typeof(string));
            dataTable.Columns.Add("email", typeof(string));
            dataTable.Columns.Add("developer", typeof(bool));
            dataTable.Columns.Add("created_at", typeof(DateTime));
            dataTable.Columns.Add("last_login", typeof(DateTime));

            // Simulate a valid user row
            dataTable.Rows.Add(1, "user1", "user1@example.com", true, DateTime.Now, DateTime.Now.AddDays(-1));

            // Act
            User? user = usersRepository.MapDataRowToUser(dataTable.Rows[0]);

            // Assert
            Assert.That(user, Is.Not.Null);
            Assert.That(user?.UserId, Is.EqualTo(1));
            Assert.That(user?.Username, Is.EqualTo("user1"));
            Assert.That(user?.Email, Is.EqualTo("user1@example.com"));
            Assert.That(user?.IsDeveloper, Is.True);
        }

        [Test]
        public void MapDataRowToUser_MissingFields_ReturnsNull()
        {
            // Arrange
            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("username", typeof(string));
            dataTable.Columns.Add("email", typeof(string));

            // Simulate a row with missing email field
            dataTable.Rows.Add(1, "user1", DBNull.Value);

            // Act
            User? user = usersRepository.MapDataRowToUser(dataTable.Rows[0]);

            // Assert
            Assert.That(user, Is.Null);
        }

        [Test]
        public void MapDataRowToUser_DeveloperIsNull_ReturnsUserWithFalseIsDeveloper()
        {
            // Arrange
            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("username", typeof(string));
            dataTable.Columns.Add("email", typeof(string));
            dataTable.Columns.Add("developer", typeof(bool));
            dataTable.Columns.Add("created_at", typeof(DateTime));
            dataTable.Columns.Add("last_login", typeof(DateTime));

            // Simulate a row with a null developer and other fields
            dataTable.Rows.Add(1, "user1", "user1@example.com", DBNull.Value, DBNull.Value, DBNull.Value);

            // Act
            User? user = usersRepository.MapDataRowToUser(dataTable.Rows[0]);

            // Assert
            Assert.That(user?.IsDeveloper, Is.False); // Since developer was null, should be false
        }

        [Test]
        public void MapDataRowToUser_FieldsAreNull_ReturnsUserWithDefaultValues()
        {
            // Arrange
            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("username", typeof(string));
            dataTable.Columns.Add("email", typeof(string));
            dataTable.Columns.Add("developer", typeof(bool));
            dataTable.Columns.Add("created_at", typeof(DateTime));
            dataTable.Columns.Add("last_login", typeof(DateTime));

            // Simulate a row where developer, created_at, and last_login are null
            dataTable.Rows.Add(1, "user2", "user2@example.com", DBNull.Value, DBNull.Value, DBNull.Value);

            // Act
            User? user = usersRepository.MapDataRowToUser(dataTable.Rows[0]);

            // Assert
            Assert.That(user?.IsDeveloper, Is.False); // Default for DBNull
        }

        [Test]
        public void MapDataRowToUserWithPassword_ValidDataRow_ReturnsUserWithPassword()
        {
            // Arrange
            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("username", typeof(string));
            dataTable.Columns.Add("email", typeof(string));
            dataTable.Columns.Add("developer", typeof(bool));
            dataTable.Columns.Add("created_at", typeof(DateTime));
            dataTable.Columns.Add("last_login", typeof(DateTime));
            dataTable.Columns.Add("hashed_password", typeof(string));

            // Simulate a valid user row
            dataTable.Rows.Add(1, "user1", "user1@example.com", true, DateTime.Now, DateTime.Now.AddDays(-1), "hashedpassword123");

            // Act
            User? user = usersRepository.MapDataRowToUserWithPassword(dataTable.Rows[0]);

            // Assert
            Assert.That(user?.Password, Is.EqualTo("hashedpassword123"));
        }

        [Test]
        public void MapDataRowToUserWithPassword_MissingFields_ReturnsNull()
        {
            // Arrange
            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("username", typeof(string));
            dataTable.Columns.Add("email", typeof(string));
            dataTable.Columns.Add("hashed_password", typeof(string));

            // Simulate a row with missing hashed_password field
            dataTable.Rows.Add(1, "user1", "user1@example.com", DBNull.Value);

            // Act
            User? user = usersRepository.MapDataRowToUserWithPassword(dataTable.Rows[0]);

            // Assert
            Assert.That(user, Is.Null);
        }

        [Test]
        public void MapDataRowToUserWithPassword_MissingCriticalFields_ReturnsNull()
        {
            // Arrange
            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("username", typeof(string));
            dataTable.Columns.Add("email", typeof(string));
            dataTable.Columns.Add("hashed_password", typeof(string));

            // Simulate a row with missing user_id field
            dataTable.Rows.Add(DBNull.Value, "user1", "user1@example.com", "hashedpassword123");

            // Act
            User? user = usersRepository.MapDataRowToUserWithPassword(dataTable.Rows[0]);

            // Assert
            Assert.That(user, Is.Null);
        }

        [Test]
        public void MapDataRowToUserWithPassword_DeveloperIsNull_ReturnsUserWithFalseIsDeveloper()
        {
            // Arrange
            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("username", typeof(string));
            dataTable.Columns.Add("email", typeof(string));
            dataTable.Columns.Add("developer", typeof(bool));
            dataTable.Columns.Add("created_at", typeof(DateTime));
            dataTable.Columns.Add("last_login", typeof(DateTime));
            dataTable.Columns.Add("hashed_password", typeof(string));

            // Simulate a row with developer as null and other fields
            dataTable.Rows.Add(1, "user1", "user1@example.com", DBNull.Value, DBNull.Value, DBNull.Value, "hashedpassword123");

            // Act
            User? user = usersRepository.MapDataRowToUserWithPassword(dataTable.Rows[0]);

            // Assert
            Assert.That(user, Is.Not.Null);
            Assert.That(user?.UserId, Is.EqualTo(1));
            Assert.That(user?.Username, Is.EqualTo("user1"));
            Assert.That(user?.Email, Is.EqualTo("user1@example.com"));
            Assert.That(user?.IsDeveloper, Is.False); // Should be false due to DBNull
            Assert.That(user?.CreatedAt, Is.EqualTo(DateTime.MinValue)); // Should be DateTime.MinValue due to DBNull
            Assert.That(user?.LastLogin, Is.Null); // Should be null due to DBNull
            Assert.That(user?.Password, Is.EqualTo("hashedpassword123")); // Check if password is correctly mapped
        }

        [Test]
        public void UserRepository_NullDataLink_ThrowsException()
        {
            // Arrange
            DataLink dataLink = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new UsersRepository(dataLink));
        }
    }
}
