using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;
using kCura.IntegrationPoints.Core.Authentication;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Security;
using Relativity.Services.Security.Models;

namespace kCura.IntegrationPoints.Core.Tests.Authentication
{
    [TestFixture, Category("Unit")]
    public class OAuth2ClientFactoryTests : TestBase
    {
        private Mock<IHelper> _helperFake;
        private Mock<IAPILog> _loggerFake;
        private Mock<IOAuth2ClientManager> _oAuth2ClientManagerFake;
        private Mock<IRetryHandler> _retryHandlerMock;
        private Mock<IRetryHandlerFactory> _retryHandlerFactoryFake;
        private int _contextUserId;
        private string _clientName;
        private OAuth2ClientFactory _instance;
        private OAuth2Client _oauth2Client;

        public override void SetUp()
        {
            _contextUserId = 1234;
            _clientName = $"{Constants.IntegrationPoints.OAUTH2_CLIENT_NAME_PREFIX} {_contextUserId}";

            _oAuth2ClientManagerFake = new Mock<IOAuth2ClientManager>();
            _loggerFake = new Mock<IAPILog>();
            _helperFake = new Mock<IHelper>();
            _retryHandlerMock = new Mock<IRetryHandler>();

            _retryHandlerFactoryFake = new Mock<IRetryHandlerFactory>();
            _retryHandlerFactoryFake
                .Setup(x => x.Create(It.IsAny<ushort>(), It.IsAny<ushort>()))
                .Returns(_retryHandlerMock.Object);
            _helperFake
                .Setup(x => x.GetLoggerFactory().GetLogger().ForContext<OAuth2ClientFactory>())
                .Returns(_loggerFake.Object);
            _helperFake
                .Setup(x => x.GetServicesManager().CreateProxy<IOAuth2ClientManager>(It.IsAny<ExecutionIdentity>()))
                .Returns(_oAuth2ClientManagerFake.Object);

            _oauth2Client = new OAuth2Client()
            {
                ContextUser = _contextUserId,
                Name = _clientName
            };

            _instance = new OAuth2ClientFactory(_retryHandlerFactoryFake.Object, _helperFake.Object);
        }

        [Test]
        public async Task GetOauth2Client_ShouldReturnExistingOAuth2Client()
        {
            //Arrange
            _retryHandlerMock
                .Setup(x => x.ExecuteWithRetriesAsync(It.IsAny<Func<Task<OAuth2Client>>>(), It.IsAny<string>()))
                .ReturnsAsync(_oauth2Client);

            // Act
            OAuth2Client result = await _instance.GetOauth2ClientAsync(_contextUserId).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            _clientName.Should().Be(result.Name);
            _contextUserId.Should().Be(result.ContextUser);
            _retryHandlerMock
                .Verify(x => x.ExecuteWithRetriesAsync(It.IsAny<Func<Task<OAuth2Client>>>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task GetOauth2Client_ShouldCreateNewOAuth2Client()
        {
            // Arrange
            _retryHandlerMock
                .SetupSequence(x => x.ExecuteWithRetriesAsync(It.IsAny<Func<Task<OAuth2Client>>>(), It.IsAny<string>()))
                .ReturnsAsync(null)
                .ReturnsAsync(_oauth2Client);

            // Act
            OAuth2Client result = await _instance.GetOauth2ClientAsync(_contextUserId).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            _clientName.Should().Be(result.Name);
            _contextUserId.Should().Be(result.ContextUser);
            _retryHandlerMock
                .Verify(x => x.ExecuteWithRetriesAsync(It.IsAny<Func<Task<OAuth2Client>>>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Test]
        public void GetOauth2Client_ShouldLogError_WhenCreatingAuthClientRetrievalFails()
        {
            // Arrange
            var exception = new OutOfMemoryException("Test exception message");
            _retryHandlerMock
                .SetupSequence(x => x.ExecuteWithRetriesAsync(It.IsAny<Func<Task<OAuth2Client>>>(), It.IsAny<string>()))
                .ReturnsAsync(null)
                .ThrowsAsync(exception);

            // Act
            Func<Task> action = () => _instance.GetOauth2ClientAsync(_contextUserId);

            //Assert
            action
                .ShouldThrow<InvalidOperationException>()
                .WithMessage($"Failed to retrieve OAuth2Client for user with id: {_contextUserId}")
                .WithInnerExceptionExactly<OutOfMemoryException>();
            _loggerFake
                .Verify(x => x.LogError($"IOAuth2ClientManager failed on CreateAsync with {exception.Message}"));
        }

        [Test]
        public void GetOauth2Client_ShouldLogError_WhenReadingAuthClientRetrievalFails()
        {
            // Arrange
            var exception = new OutOfMemoryException("Test exception message");
            _retryHandlerMock
                .Setup(x => x.ExecuteWithRetriesAsync(It.IsAny<Func<Task<OAuth2Client>>>(), It.IsAny<string>()))
                .ThrowsAsync(exception);

            // Act
            Func<Task> action = () => _instance.GetOauth2ClientAsync(_contextUserId);

            //Assert
            action
                .ShouldThrow<InvalidOperationException>()
                .WithMessage($"Failed to retrieve OAuth2Client for user with id: {_contextUserId}")
                .WithInnerExceptionExactly<OutOfMemoryException>();
            _loggerFake
                .Verify(x => x.LogError($"IOAuth2ClientManager failed on ReadAllAsync with { exception.Message}"));
        }
    }
}