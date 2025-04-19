using System.Collections.Generic;
using System;
using System.Data;
using System.Linq;
using BusinessLayer.Data;
using BusinessLayer.Exceptions;
using BusinessLayer.Repositories;
using BusinessLayer.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Moq;
using NUnit.Framework;

namespace Tests.RepositoryTests
{
    [TestFixture]
    public class PasswordResetRepositoryTests
    {
        private Mock<IDataLink> mockDataLink;
        private IPasswordResetRepository passwordResetRepository;

        [SetUp]
        public void Setup()
        {
            this.mockDataLink = new Mock<IDataLink>();
            this.passwordResetRepository = new PasswordResetRepository(this.mockDataLink.Object);
        }

        [Test]
        public void Constructor_WithNullDataLink_ThrowsArgumentNullException()
        {
            // Act & Assert
            try
            {
                new PasswordResetRepository(null);
                Assert.Fail("Expected ArgumentNullException was not thrown");
            }
            catch (ArgumentNullException)
            {
                // Expected
                Assert.Pass();
            }
        }

        [Test]
        public void StoreResetCode_DeleteExistingResetCodesWasCalled()
        {
            // Arrange
            int userId = 1;
            string code = "123456";
            DateTime expiryTime = DateTime.Now.AddMinutes(30);
            string deleteProcedure = "DeleteExistingResetCodes";

            this.mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery(deleteProcedure, It.Is<SqlParameter[]>(sqlParameter => sqlParameter.Length == 1 && (int)sqlParameter[0].Value == userId)))
                .Verifiable();

            this.mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery("StorePasswordResetCode", It.Is<SqlParameter[]>(sqlParameter => sqlParameter.Length == 3)))
                .Verifiable();

            // Act
            this.passwordResetRepository.StoreResetCode(userId, code, expiryTime);

            // Assert
            bool deleteProcedureCalled = false;
            foreach (var invocation in this.mockDataLink.Invocations)
            {
                if (invocation.Method.Name == "ExecuteNonQuery")
                {
                    if ((string)invocation.Arguments[0] == deleteProcedure)
                    {
                        deleteProcedureCalled = true;
                        break;
                    }
                }
            }

            Assert.That(deleteProcedureCalled, Is.True, "DeleteExistingResetCodes was not called");
        }

        [Test]
        public void StoreResetCode_StorePasswordResetCodeWasCalled()
        {
            // Arrange
            int userId = 1;
            string code = "123456";
            DateTime expiryTime = DateTime.Now.AddMinutes(30);
            string storeProcedure = "StorePasswordResetCode";

            this.mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery("DeleteExistingResetCodes", It.Is<SqlParameter[]>(sqlParameter => sqlParameter.Length == 1)))
                .Verifiable();

            this.mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery(storeProcedure, It.Is<SqlParameter[]>(sqlParameter => sqlParameter.Length == 3)))
                .Verifiable();

            // Act
            this.passwordResetRepository.StoreResetCode(userId, code, expiryTime);

            // Assert
            bool storeProcedureCalled = false;
            foreach (var invocation in this.mockDataLink.Invocations)
            {
                if (invocation.Method.Name == "ExecuteNonQuery")
                {
                    if ((string)invocation.Arguments[0] == storeProcedure)
                    {
                        storeProcedureCalled = true;
                        break;
                    }
                }
            }

            Assert.That(storeProcedureCalled, Is.True, "StorePasswordResetCode was not called");
        }

        [Test]
        public void StoreResetCode_WhenDatabaseOperationExceptionOccurs_ThrowsRepositoryException()
        {
            // Arrange
            int userId = 1;
            string code = "123456";
            DateTime expiryTime = DateTime.Now.AddMinutes(30);
            string deleteProcedure = "DeleteExistingResetCodes";

            this.mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery(deleteProcedure, It.Is<SqlParameter[]>(sqlParameter => sqlParameter.Length == 1)))
                .Throws(new DatabaseOperationException("Database error"));

            // Act & Assert
            try
            {
                this.passwordResetRepository.StoreResetCode(userId, code, expiryTime);
                Assert.Fail("Expected RepositoryException was not thrown");
            }
            catch (RepositoryException)
            {
                // Expected
                Assert.Pass();
            }
        }

        [Test]
        public void VerifyResetCode_WithValidCode_ReturnsTrue()
        {
            // Arrange
            string email = "test@example.com";
            string code = "123456";
            var dataTable = new DataTable();
            dataTable.Columns.Add("expiration_time", typeof(DateTime));
            dataTable.Columns.Add("used", typeof(bool));
            dataTable.Rows.Add(DateTime.UtcNow.AddMinutes(10), false); // Valid code

            this.mockDataLink.Setup(dataLink => dataLink.ExecuteReader(
                It.IsAny<string>(),
                It.Is<SqlParameter[]>(sqlParameter =>
                    sqlParameter.Length == 2 &&
                    (string)sqlParameter[0].Value == email &&
                    (string)sqlParameter[1].Value == code))).Returns(dataTable);

            // Act
            bool result = this.passwordResetRepository.VerifyResetCode(email, code);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void VerifyResetCode_WithInvalidCode_ReturnsFalse()
        {
            // Arrange
            string email = "test@example.com";
            string code = "123456";
            var dataTable = new DataTable();
            dataTable.Columns.Add("expiration_time", typeof(DateTime));
            dataTable.Columns.Add("used", typeof(bool));
            dataTable.Rows.Add(DateTime.UtcNow.AddMinutes(-10), false); // Expired code

            this.mockDataLink.Setup(dataLink => dataLink.ExecuteReader(
                It.IsAny<string>(),
                It.Is<SqlParameter[]>(sqlParameter =>
                    sqlParameter.Length == 2 &&
                    (string)sqlParameter[0].Value == email &&
                    (string)sqlParameter[1].Value == code))).Returns(dataTable);

            // Act
            bool result = this.passwordResetRepository.VerifyResetCode(email, code);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void VerifyResetCode_WithUsedCode_ReturnsFalse()
        {
            // Arrange
            string email = "test@example.com";
            string code = "123456";
            var dataTable = new DataTable();
            dataTable.Columns.Add("expiration_time", typeof(DateTime));
            dataTable.Columns.Add("used", typeof(bool));
            dataTable.Rows.Add(DateTime.UtcNow.AddMinutes(10), true); // Used code

            this.mockDataLink.Setup(dataLink => dataLink.ExecuteReader(
                It.IsAny<string>(),
                It.Is<SqlParameter[]>(sqlParameter =>
                    sqlParameter.Length == 2 &&
                    (string)sqlParameter[0].Value == email &&
                    (string)sqlParameter[1].Value == code))).Returns(dataTable);

            // Act
            bool result = this.passwordResetRepository.VerifyResetCode(email, code);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void VerifyResetCode_WithNoCode_ReturnsFalse()
        {
            // Arrange
            string email = "test@example.com";
            string code = "123456";
            var dataTable = new DataTable();
            dataTable.Columns.Add("expiration_time", typeof(DateTime));
            dataTable.Columns.Add("used", typeof(bool));

            // No rows added
            this.mockDataLink.Setup(dataLink => dataLink.ExecuteReader(
                It.IsAny<string>(),
                It.Is<SqlParameter[]>(sqlParameter =>
                    sqlParameter.Length == 2 &&
                    (string)sqlParameter[0].Value == email &&
                    (string)sqlParameter[1].Value == code))).Returns(dataTable);

            // Act
            bool result = this.passwordResetRepository.VerifyResetCode(email, code);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void VerifyResetCode_WhenDatabaseOperationExceptionOccurs_ThrowsRepositoryException()
        {
            // Arrange
            string email = "test@example.com";
            string code = "123456";

            this.mockDataLink.Setup(dataLink => dataLink.ExecuteReader(
                It.IsAny<string>(),
                It.Is<SqlParameter[]>(sqlParameter => sqlParameter.Length == 2))).Throws(new DatabaseOperationException("Database error"));

            // Act & Assert
            try
            {
                this.passwordResetRepository.VerifyResetCode(email, code);
                Assert.Fail("Expected RepositoryException was not thrown");
            }
            catch (RepositoryException)
            {
                // Expected
                Assert.Pass();
            }
        }

        [Test]
        public void ResetPassword_WithValidCodeAndEmail_UpdatesPassword()
        {
            // Arrange
            string email = "test@example.com";
            string code = "123456";
            string newPassword = "NewPassword123!";
            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));
            dataTable.Rows.Add(1); // Valid user

            this.mockDataLink.Setup(dataLink => dataLink.ExecuteReader(
                It.IsAny<string>(),
                It.Is<SqlParameter[]>(sqlParameter => sqlParameter.Length == 2))).Returns(dataTable);

            this.mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery(
                It.IsAny<string>(),
                It.Is<SqlParameter[]>(sqlParameter => sqlParameter.Length == 2))).Returns(1);

            // Act
            bool result = this.passwordResetRepository.ResetPassword(email, code, newPassword);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ResetPassword_WithInvalidCode_ReturnsFalse()
        {
            // Arrange
            string email = "test@example.com";
            string code = "123456";
            string newPassword = "NewPassword123!";
            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));

            // No rows added - invalid code
            this.mockDataLink.Setup(dataLink => dataLink.ExecuteReader(
                It.IsAny<string>(),
                It.Is<SqlParameter[]>(sqlParameter => sqlParameter.Length == 2))).Returns(dataTable);

            // Act
            bool result = this.passwordResetRepository.ResetPassword(email, code, newPassword);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ResetPassword_WithNonExistentUser_ReturnsFalse()
        {
            // Arrange
            string email = "nonexistent@example.com";
            string code = "123456";
            string newPassword = "NewPassword123!";
            var dataTable = new DataTable();
            dataTable.Columns.Add("user_id", typeof(int));

            // No rows added - non-existent user
            this.mockDataLink.Setup(dataLink => dataLink.ExecuteReader(
                It.IsAny<string>(),
                It.Is<SqlParameter[]>(sqlParameter => sqlParameter.Length == 2))).Returns(dataTable);

            // Act
            bool result = this.passwordResetRepository.ResetPassword(email, code, newPassword);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ResetPassword_WhenDatabaseOperationExceptionOccurs_ThrowsRepositoryException()
        {
            // Arrange
            string email = "test@example.com";
            string code = "123456";
            string newPassword = "NewPassword123!";

            this.mockDataLink.Setup(dataLink => dataLink.ExecuteReader(
                It.IsAny<string>(),
                It.Is<SqlParameter[]>(sqlParameter => sqlParameter.Length == 2))).Throws(new DatabaseOperationException("Database error"));

            // Act & Assert
            try
            {
                this.passwordResetRepository.ResetPassword(email, code, newPassword);
                Assert.Fail("Expected RepositoryException was not thrown");
            }
            catch (RepositoryException)
            {
                // Expected
                Assert.Pass();
            }
        }

        [Test]
        public void CleanupExpiredCodes_ExecutesCorrectProcedure()
        {
            // Arrange
            string procedureName = "CleanupExpiredResetCodes";

            this.mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery(procedureName, It.Is<SqlParameter[]>(sqlParameter => sqlParameter.Length == 0)))
                .Verifiable();

            // Act
            this.passwordResetRepository.CleanupExpiredCodes();

            // Assert
            bool procedureCalled = false;
            foreach (var invocation in this.mockDataLink.Invocations)
            {
                if (invocation.Method.Name == "ExecuteNonQuery")
                {
                    if ((string)invocation.Arguments[0] == procedureName)
                    {
                        procedureCalled = true;
                        break;
                    }
                }
            }

            Assert.That(procedureCalled, Is.True, "CleanupExpiredResetCodes was not called.");
        }

        [Test]
        public void CleanupExpiredCodes_WhenDatabaseOperationExceptionOccurs_ThrowsRepositoryException()
        {
            // Arrange
            string procedureName = "CleanupExpiredResetCodes";

            this.mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery(procedureName, It.Is<SqlParameter[]>(sqlParameter => sqlParameter.Length == 0)))
                .Throws(new DatabaseOperationException("Database error"));

            // Act & Assert
            try
            {
                this.passwordResetRepository.CleanupExpiredCodes();
                Assert.Fail("Expected RepositoryException was not thrown.");
            }
            catch (RepositoryException)
            {
                // Expected
                Assert.Pass();
            }
        }
    }
}