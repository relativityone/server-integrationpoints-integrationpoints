using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Models;
using kCura.IntegrationPoints.Data.Facades.SecretStore;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class SecretsRepositoryTests
    {
        private Mock<IAPILog> _loggerMock;
        private Mock<ISecretStoreFacade> _secretStoreMock;
        private SecretsRepository _sut;
        private const int _WORKSPACE_ID = 1001;
        private const int _INTEGRATION_POINT_ID = 2002;
        private readonly SecretPath _testSecretPath = SecretPath.ForIntegrationPointSecret(
            _WORKSPACE_ID,
            _INTEGRATION_POINT_ID,
            secretID: "c2c33312-66b9-4604-980b-a510aebc8f0e"
        );

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<IAPILog>();
            _loggerMock
                .Setup(x => x.ForContext<SecretsRepository>())
                .Returns(_loggerMock.Object);
            _secretStoreMock = new Mock<ISecretStoreFacade>();

            _sut = new SecretsRepository(
                _secretStoreMock.Object,
                _loggerMock.Object
            );
        }

        [Test]
        public async Task EncryptAsync_ShouldReturnSecretIDAndCallSecretStoreOnceWhenCorrectSecretPathPassed()
        {
            // arrange
            var expectedSecretData = new Dictionary<string, string>
            {
                ["SecuredConfiguration"] = "TestSecret"
            };

            // act
            string secretID = await _sut.EncryptAsync(
                    _testSecretPath,
                    expectedSecretData
                ).ConfigureAwait(false);

            // assert
            secretID.Should().Be(_testSecretPath.SecretID);
            _secretStoreMock.Verify(x => x.SetAsync(
                    It.Is<string>(secretPath => secretPath == _testSecretPath.ToString()),
                    It.Is<Secret>(secret => secret.Data.Count == expectedSecretData.Count
                        && secret.Data["SecuredConfiguration"] == "TestSecret")
                ), Times.Once);
        }

        [Test]
        public void EncryptAsync_ShouldThrowWhenNullSecretPathPassed()
        {
            // arrange
            var expectedSecretData = new Dictionary<string, string>();

            // act
            Func<Task> encryptAction = () => _sut.EncryptAsync(
                secretPath: null,
                secretData: expectedSecretData
            );

            // assert
            encryptAction.ShouldThrow<ArgumentException>().WithMessage("Secret path cannot be null");
        }

        [Test]
        public void EncryptAsync_ShouldThrowWhenSecretStoreThrows()
        {
            // arrange
            var exception = new Exception();
            _secretStoreMock
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<Secret>()))
                .Throws(exception);
            var expectedSecretData = new Dictionary<string, string>();

            // act
            Func<Task> encryptAction = () => _sut.EncryptAsync(_testSecretPath, expectedSecretData);

            // assert
            encryptAction.ShouldThrow<Exception>().Which.Should().Be(exception);
        }

        [Test]
        public async Task DecryptAsync_ShouldReturnSecretDataAndCallSecretStoreOnceWhenCorrectSecretPathPassed()
        {
            // arrange
            var expectedSecretData = new Dictionary<string, string>
            {
                ["SecuredConfiguration"] = "TestSecret"
            };
            var secret = new Secret
            {
                Data = expectedSecretData
            };
            _secretStoreMock
                .Setup(x => x.GetAsync(_testSecretPath.ToString()))
                .ReturnsAsync(secret);

            // act
            Dictionary<string, string> secretData = await _sut
                .DecryptAsync(_testSecretPath)
                .ConfigureAwait(false);

            // assert
            secretData.ShouldAllBeEquivalentTo(expectedSecretData);

            _secretStoreMock.Verify(x => x.GetAsync(
                It.Is<string>(secretPath => secretPath == _testSecretPath.ToString())
            ), Times.Once);
        }

        [Test]
        public void DecryptAsync_ShouldThrowWhenNullSecretPathPassed()
        {
            // act
            Func<Task> decryptAction = () => _sut.DecryptAsync(secretPath: null);

            // assert
            decryptAction.ShouldThrow<ArgumentException>().WithMessage("Secret path cannot be null");
        }

        [Test]
        public async Task DecryptAsync_ShouldReturnNullAndWarningShouldBeLoggedWhenSecretStoreThrows()
        {
            // arrange
            var exception = new Exception("test");
            _secretStoreMock
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ThrowsAsync(exception);

            // act
            Dictionary<string, string> secretData = await _sut
                .DecryptAsync(_testSecretPath)
                .ConfigureAwait(false);

            // assert
            secretData.Should().BeNull();
            _loggerMock.Verify(x => x.LogWarning(exception, It.IsAny<string>()));
        }

        [Test]
        public async Task DeleteAsync_ShouldCallSecretStoreOnceWhenCorrectSecretPathPassed()
        {
            // act
            await _sut.DeleteAsync(_testSecretPath).ConfigureAwait(false);

            // assert
            _secretStoreMock.Verify(x => x.DeleteAsync(
                It.Is<string>(secretPath => secretPath == _testSecretPath.ToString())
            ), Times.Once);
        }

        [Test]
        public void DeleteAsync_ShouldThrowWhenNullSecretPathPassed()
        {
            // act
            Func<Task> deleteAction = () => _sut.DeleteAsync(secretPath: null);

            // assert
            deleteAction.ShouldThrow<ArgumentException>().WithMessage("Secret path cannot be null");
        }

        [Test]
        public void DeleteAsync_ShouldThrowWhenSecretStoreThrows()
        {
            // arrange
            var exception = new Exception();
            _secretStoreMock
                .Setup(x => x.DeleteAsync(It.IsAny<string>()))
                .Throws(exception);

            // act
            Func<Task> deleteAction = () => _sut.DeleteAsync(_testSecretPath);

            // assert
            deleteAction.ShouldThrow<Exception>().Which.Should().Be(exception);
        }

        [Test]
        public async Task DeleteAllRipSecretsFromAllWorkspacesAsync_ShouldCallSecretStoreOnceWhenCorrectSecretPathPassed()
        {
            // arrange
            string expectedSecretPath = string.Empty;

            // act
            await _sut.DeleteAllRipSecretsFromAllWorkspacesAsync().ConfigureAwait(false);

            // assert
            _secretStoreMock.Verify(x => x.DeleteAsync(
                It.Is<string>(secretPath => secretPath == expectedSecretPath)
            ), Times.Once);
        }

        [Test]
        public void DeleteAllRipSecretsFromAllWorkspacesAsync_ShouldThrowWhenSecretStoreThrows()
        {
            // arrange
            var exception = new Exception();
            _secretStoreMock
                .Setup(x => x.DeleteAsync(It.IsAny<string>()))
                .Throws(exception);

            // act
            Func<Task> deleteAction = () => _sut.DeleteAllRipSecretsFromAllWorkspacesAsync();

            // assert
            deleteAction.ShouldThrow<Exception>().Which.Should().Be(exception);
        }

        [Test]
        public async Task DeleteAllRipSecretsFromWorkspaceAsync_ShouldCallSecretStoreOnce()
        {
            // arrange
            string expectedSecretPath = $"/{_WORKSPACE_ID}";

            // act
            await _sut.DeleteAllRipSecretsFromWorkspaceAsync(_WORKSPACE_ID)
                .ConfigureAwait(false);

            // assert
            _secretStoreMock.Verify(x => x.DeleteAsync(
                It.Is<string>(secretPath => secretPath == expectedSecretPath)
            ), Times.Once);
        }

        [Test]
        public void DeleteAllRipSecretsFromWorkspaceAsync_ShouldThrowWhenSecretStoreThrows()
        {
            // arrange
            var exception = new Exception();
            _secretStoreMock
                .Setup(x => x.DeleteAsync(It.IsAny<string>()))
                .Throws(exception);

            // act
            Func<Task> deleteAction = () => _sut.DeleteAllRipSecretsFromWorkspaceAsync(_WORKSPACE_ID);

            // assert
            deleteAction.ShouldThrow<Exception>().Which.Should().Be(exception);
        }

        [Test]
        public async Task DeleteAllRipSecretsFromIntegrationPointAsync_ShouldCallSecretStoreOnce()
        {
            // arrange
            string expectedSecretPath = $"/{_WORKSPACE_ID}/{_INTEGRATION_POINT_ID}";

            // act
            await _sut.DeleteAllRipSecretsFromIntegrationPointAsync(
                _WORKSPACE_ID,
                _INTEGRATION_POINT_ID
            ).ConfigureAwait(false);

            // assert
            _secretStoreMock.Verify(x => x.DeleteAsync(
                It.Is<string>(secretPath => secretPath == expectedSecretPath)
            ), Times.Once);
        }

        [Test]
        public void DeleteAllRipSecretsFromIntegrationPointAsync_ShouldThrowWhenSecretStoreThrows()
        {
            // arrange
            var exception = new Exception();
            _secretStoreMock
                .Setup(x => x.DeleteAsync(It.IsAny<string>()))
                .Throws(exception);

            // act
            Func<Task> deleteAction = () => _sut.DeleteAllRipSecretsFromIntegrationPointAsync(
                _WORKSPACE_ID,
                _INTEGRATION_POINT_ID
            );

            // assert
            deleteAction.ShouldThrow<Exception>().Which.Should().Be(exception);
        }
    }
}
