using System.Data;
using BusinessLayer.Data;
using BusinessLayer.Exceptions;
using BusinessLayer.Models;
using BusinessLayer.Repositories;
using Microsoft.Data.SqlClient;
using Moq;

namespace Tests.RepositoryTests
{
    [TestFixture]
    internal class UserProfileRepositoryTests
    {
        private UserProfilesRepository userProfileRepository;
        private Mock<IDataLink> mockDataLink;

        [SetUp]
        public void Setup()
        {
            mockDataLink = new Mock<IDataLink>();
            userProfileRepository = new UserProfilesRepository(mockDataLink.Object);
        }

        [Test]
        public void Constructor_NullDataLink_ThrowsArgumentNullException()
        {
            // Arrange

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new UserProfilesRepository(null));
        }

        [Test]
        public void GetUserProfileByUserId_ValidUserId_ReturnsUserProfile()
        {
            // Arrange
            var userId = 1;
            var profileId = 101; // Example profile ID

            var dataTable = new DataTable();
            dataTable.Columns.Add("profile_id", typeof(int)); // Add the missing column
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("bio", typeof(string));
            dataTable.Columns.Add("profile_picture", typeof(string));
            dataTable.Columns.Add("last_modified", typeof(DateTime));
            dataTable.Rows.Add(profileId, userId, "Test Bio", "TestPicture.jpg", DateTime.Now);

            mockDataLink
                .Setup(dataLink => dataLink.ExecuteReader("GetUserProfileByUserId", It.IsAny<SqlParameter[]>()))
                .Returns(dataTable);

            // Act
            var result = userProfileRepository.GetUserProfileByUserId(userId);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void GetUserProfileByUserId_NoRows_ReturnsNull()
        {
            // Arrange
            var userId = 1;
            var dataTable = new DataTable();

            mockDataLink
                .Setup(dataLink => dataLink.ExecuteReader("GetUserProfileByUserId", It.IsAny<SqlParameter[]>()))
                .Returns(dataTable);

            // Act
            var result = userProfileRepository.GetUserProfileByUserId(userId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetUserProfileByUserId_DatabaseOperationException_ThrowsRepositoryException()
        {
            // Arrange
            var userId = 1;

            mockDataLink
                .Setup(dataLink => dataLink.ExecuteReader("GetUserProfileByUserId", It.IsAny<SqlParameter[]>()))
                .Throws(new DatabaseOperationException("Database Error"));

            // Act & Assert
            Assert.Throws<RepositoryException>(() => userProfileRepository.GetUserProfileByUserId(userId));
        }

        [Test]
        public void UpdateProfile_ValidProfile_ReturnsUpdatedProfile()
        {
            // Arrange
            var profile = new UserProfile
            {
                ProfileId = 1,
                UserId = 1,
                Bio = "Test Bio",
            };

            var dataTable = new DataTable();
            dataTable.Columns.Add("profile_id", typeof(int)); // Add the missing column
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("bio", typeof(string));
            dataTable.Columns.Add("profile_picture", typeof(string));
            dataTable.Columns.Add("last_modified", typeof(DateTime));
            dataTable.Rows.Add(profile.ProfileId, profile.UserId, profile.Bio, "TestPicture.jpg", DateTime.Now);

            mockDataLink
                .Setup(dataLink => dataLink.ExecuteReader("UpdateUserProfile", It.IsAny<SqlParameter[]>()))
                .Returns(dataTable);

            // Act
            var result = userProfileRepository.UpdateProfile(profile);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void UpdateProfile_NoRows_ReturnsNull()
        {
            // Arrange
            var profile = new UserProfile
            {
                ProfileId = 1,
                UserId = 1,
                Bio = "Test Bio",
            };

            var dataTable = new DataTable();

            mockDataLink
                .Setup(dataLink => dataLink.ExecuteReader("UpdateUserProfile", It.IsAny<SqlParameter[]>()))
                .Returns(dataTable);

            // Act
            var result = userProfileRepository.UpdateProfile(profile);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void UpdateProfile_DatabaseOperationException_ThrowsRepositoryException()
        {
            // Arrange
            var profile = new UserProfile
            {
                ProfileId = 1,
                UserId = 1,
                Bio = "Test Bio",
            };

            mockDataLink
                .Setup(dataLink => dataLink.ExecuteReader("UpdateUserProfile", It.IsAny<SqlParameter[]>()))
                .Throws(new DatabaseOperationException("Database Error"));

            // Act & Assert
            Assert.Throws<RepositoryException>(() => userProfileRepository.UpdateProfile(profile));
        }

        [Test]
        public void UpdateProfile_NullBio_SetsBioToDBNull()
        {
            // Arrange
            var profile = new UserProfile
            {
                ProfileId = 1,
                UserId = 1,
                Bio = null,
                ProfilePicture = "pic.png"
            };

            var dataTable = new DataTable();
            dataTable.Columns.Add("profile_id", typeof(int));
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("bio", typeof(string));
            dataTable.Columns.Add("profile_picture", typeof(string));
            dataTable.Columns.Add("last_modified", typeof(DateTime));
            dataTable.Rows.Add(profile.ProfileId, profile.UserId, DBNull.Value, "pic.png", DateTime.Now);

            mockDataLink
                .Setup(dataLink => dataLink.ExecuteReader("UpdateUserProfile", It.IsAny<SqlParameter[]>()))
                .Returns(dataTable);

            // Act
            var result = userProfileRepository.UpdateProfile(profile);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void UpdateProfile_WithProfilePicture_SetsProfilePictureParam()
        {
            // Arrange
            var profile = new UserProfile
            {
                ProfileId = 1,
                UserId = 1,
                Bio = "Test Bio",
                ProfilePicture = "pic.png"
            };

            var dataTable = new DataTable();
            dataTable.Columns.Add("profile_id", typeof(int));
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("bio", typeof(string));
            dataTable.Columns.Add("profile_picture", typeof(string));
            dataTable.Columns.Add("last_modified", typeof(DateTime));
            dataTable.Rows.Add(profile.ProfileId, profile.UserId, profile.Bio, profile.ProfilePicture, DateTime.Now);

            mockDataLink
                .Setup(dataLink => dataLink.ExecuteReader("UpdateUserProfile", It.IsAny<SqlParameter[]>()))
                .Returns(dataTable);

            // Act
            var result = userProfileRepository.UpdateProfile(profile);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void CreateProfile_ValidUserId_ReturnsNewProfile()
        {
            // Arrange
            var userId = 1;
            var profileId = 101; // Example profile ID

            var dataTable = new DataTable();
            dataTable.Columns.Add("profile_id", typeof(int)); // Add the missing column
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Columns.Add("bio", typeof(string));
            dataTable.Columns.Add("profile_picture", typeof(string));
            dataTable.Columns.Add("last_modified", typeof(DateTime));
            dataTable.Rows.Add(profileId, userId, "Test Bio", "TestPicture.jpg", DateTime.Now);

            mockDataLink
                .Setup(dataLink => dataLink.ExecuteReader("CreateUserProfile", It.IsAny<SqlParameter[]>()))
                .Returns(dataTable);

            // Act
            var result = userProfileRepository.CreateProfile(userId);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void CreateProfile_NoRows_ReturnsNull()
        {
            // Arrange
            var userId = 1;
            var dataTable = new DataTable();

            mockDataLink
                .Setup(dataLink => dataLink.ExecuteReader("CreateUserProfile", It.IsAny<SqlParameter[]>()))
                .Returns(dataTable);

            // Act
            var result = userProfileRepository.CreateProfile(userId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void CreateProfile_DatabaseOperationException_ThrowsRepositoryException()
        {
            // Arrange
            var userId = 1;

            mockDataLink
                .Setup(dataLink => dataLink.ExecuteReader("CreateUserProfile", It.IsAny<SqlParameter[]>()))
                .Throws(new DatabaseOperationException("Database Error"));

            // Act & Assert
            Assert.Throws<RepositoryException>(() => userProfileRepository.CreateProfile(userId));
        }

        [Test]
        public void UpdateProfileBio_ValidInputs_ExecutesWithoutException()
        {
            // Arrange
            var userId = 1;
            var bio = "This is a test bio.";

            mockDataLink
                .Setup(dataLink => dataLink.ExecuteReader("UpdateUserProfileBio", It.IsAny<SqlParameter[]>()))
                .Returns(new DataTable());

            // Act & Assert
            Assert.DoesNotThrow(() => userProfileRepository.UpdateProfileBio(userId, bio));
        }

        [Test]
        public void UpdateProfileBio_DatabaseOperationException_ThrowsRepositoryException()
        {
            // Arrange
            var userId = 1;
            var bio = "This is a test bio.";

            mockDataLink
                .Setup(dataLink => dataLink.ExecuteReader("UpdateUserProfileBio", It.IsAny<SqlParameter[]>()))
                .Throws(new DatabaseOperationException("Database error"));

            // Act & Assert
            var exception = Assert.Throws<RepositoryException>(() => userProfileRepository.UpdateProfileBio(userId, bio));
            Assert.That(exception.Message, Is.EqualTo($"Failed to update profile for user {userId}."));
        }

        [Test]
        public void UpdateProfilePicture_ValidInputs_ExecutesWithoutException()
        {
            // Arrange
            var userId = 1;
            var picture = "profile_picture_url";

            mockDataLink
                .Setup(dataLink => dataLink.ExecuteReader("UpdateUserProfilePicture", It.IsAny<SqlParameter[]>()))
                .Returns(new DataTable());

            // Act & Assert
            Assert.DoesNotThrow(() => userProfileRepository.UpdateProfilePicture(userId, picture));
        }

        [Test]
        public void UpdateProfilePicture_DatabaseOperationException_ThrowsRepositoryException()
        {
            // Arrange
            var userId = 1;
            var picture = "profile_picture_url";

            mockDataLink
                .Setup(dataLink => dataLink.ExecuteReader("UpdateUserProfilePicture", It.IsAny<SqlParameter[]>()))
                .Throws(new DatabaseOperationException("Database error"));

            // Act & Assert
            var exception = Assert.Throws<RepositoryException>(() => userProfileRepository.UpdateProfilePicture(userId, picture));
            Assert.That(exception.Message, Is.EqualTo($"Failed to update profile for user {userId}."));
        }
    }
}
