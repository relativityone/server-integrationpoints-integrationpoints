using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Security;
using Relativity.Services.Security.Models;
using Relativity.Sync.Authentication;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Authentication
{
    [TestFixture]
    public class OAuth2ClientFactoryTests
    {
        private Mock<ISourceServiceFactoryForAdmin> _serviceFactoryForAdmin;
        private OAuth2ClientFactory _sut;

        private const string _OAUTH2_CLIENT_NAME_PREFIX = "F6B8C2B4B3E8465CA00775F699375D3C";

        [OneTimeSetUp]
        public void SetUp()
        {
            _serviceFactoryForAdmin = new Mock<ISourceServiceFactoryForAdmin>();
            _sut = new OAuth2ClientFactory(_serviceFactoryForAdmin.Object, new EmptyLogger());
        }

        [Test]
        public async Task ItShouldReturnExistingAuthClient()
        {
            const int userId = 1;
            string clientName = $"{_OAUTH2_CLIENT_NAME_PREFIX} {userId}";
            Services.Security.Models.OAuth2Client expectedClient = new Services.Security.Models.OAuth2Client() { Name = clientName };

            Mock<IOAuth2ClientManager> clientManager = new Mock<IOAuth2ClientManager>();
            clientManager.Setup(x => x.ReadAllAsync()).ReturnsAsync(new List<Services.Security.Models.OAuth2Client>() {expectedClient});
            _serviceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IOAuth2ClientManager>()).Returns(Task.FromResult(clientManager.Object));

            // act
            Services.Security.Models.OAuth2Client actualClient = await _sut.GetOauth2ClientAsync(userId).ConfigureAwait(false);

            // assert
            actualClient.Should().BeEquivalentTo(expectedClient);
            clientManager.Verify(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<OAuth2Flow>(), It.IsAny<IEnumerable<Uri>>(), It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public async Task ItShouldCreateNewAuthClient()
        {
            const int userId = 1;
            string clientName = $"{_OAUTH2_CLIENT_NAME_PREFIX} {userId}";
            Services.Security.Models.OAuth2Client expectedClient = new Services.Security.Models.OAuth2Client() { Name = clientName };

            Mock<IOAuth2ClientManager> clientManager = new Mock<IOAuth2ClientManager>();
            clientManager.Setup(x => x.ReadAllAsync()).ReturnsAsync(Enumerable.Empty<Services.Security.Models.OAuth2Client>().ToList());
            clientManager.Setup(x => x.CreateAsync(clientName, OAuth2Flow.ClientCredentials, It.IsAny<IEnumerable<Uri>>(), userId)).ReturnsAsync(expectedClient);
            _serviceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IOAuth2ClientManager>()).Returns(Task.FromResult(clientManager.Object));

            // act
            Services.Security.Models.OAuth2Client actualClient = await _sut.GetOauth2ClientAsync(userId).ConfigureAwait(false);

            // assert
            actualClient.Should().BeEquivalentTo(expectedClient);
            clientManager.Verify(x => x.CreateAsync(clientName, OAuth2Flow.ClientCredentials, It.IsAny<IEnumerable<Uri>>(), userId),
                Times.Once);
        }

        [Test]
        public void ItShouldThrowInvalidOperationExceptionWhenReadAllAsyncFails()
        {
            const int userId = 1;
            Mock<IOAuth2ClientManager> clientManager = new Mock<IOAuth2ClientManager>();
            clientManager.Setup(x => x.ReadAllAsync()).Throws<Exception>();
            _serviceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IOAuth2ClientManager>()).Returns(Task.FromResult(clientManager.Object));

            // act
            Func<Task> action = async () => await _sut.GetOauth2ClientAsync(userId).ConfigureAwait(false);

            // assert
            action.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void ItShouldThrowInvalidOperationExceptionWhenCreateAsyncFails()
        {
            const int userId = 1;
            Mock<IOAuth2ClientManager> clientManager = new Mock<IOAuth2ClientManager>();
            clientManager.Setup(x => x.CreateAsync(
                It.IsAny<string>(), It.IsAny<OAuth2Flow>(), It.IsAny<IEnumerable<Uri>>(), It.IsAny<int?>())).Throws<Exception>();
            _serviceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IOAuth2ClientManager>()).Returns(Task.FromResult(clientManager.Object));

            // act
            Func<Task> action = async () => await _sut.GetOauth2ClientAsync(userId).ConfigureAwait(false);

            // assert
            action.Should().Throw<InvalidOperationException>();
        }
    }
}