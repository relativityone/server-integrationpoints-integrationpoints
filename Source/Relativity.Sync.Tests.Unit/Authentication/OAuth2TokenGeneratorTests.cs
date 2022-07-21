using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.OAuth2Client.Interfaces;
using Relativity.Sync.Authentication;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Authentication
{
    [TestFixture]
    public class OAuth2TokenGeneratorTests
    {
        private Mock<IOAuth2ClientFactory> _oAuth2ClientFactory;
        private Mock<ITokenProviderFactoryFactory> _tokenProviderFactoryFactory;
        private OAuth2TokenGenerator _sut;

        [OneTimeSetUp]
        public void SetUp()
        {
            _oAuth2ClientFactory = new Mock<IOAuth2ClientFactory>();
            _tokenProviderFactoryFactory = new Mock<ITokenProviderFactoryFactory>();
            Uri uri = new Uri("https://fakeaddress");
            _sut = new OAuth2TokenGenerator(_oAuth2ClientFactory.Object, _tokenProviderFactoryFactory.Object, uri, new EmptyLogger());
        }

        [Test]
        public async Task ItShouldGenerateAuthToken()
        {
            const string authToken = "auth_token";
            const int userId = 1;
            Services.Security.Models.OAuth2Client client = new Services.Security.Models.OAuth2Client
            {
                Id = "id",
                Secret = "secret"
            };

            _oAuth2ClientFactory.Setup(x => x.GetOauth2ClientAsync(userId)).ReturnsAsync(client);

            Mock<ITokenProviderFactory> tokenProviderFactory = new Mock<ITokenProviderFactory>();
            Mock<ITokenProvider> tokenProvider = new Mock<ITokenProvider>();
            tokenProvider.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync(authToken);
            tokenProviderFactory.Setup(x => x.GetTokenProvider(It.IsAny<string>(), It.IsAny<List<string>>())).Returns(tokenProvider.Object);
            _tokenProviderFactoryFactory.Setup(x => x.Create(It.IsAny<Uri>(), client.Id, client.Secret)).Returns(tokenProviderFactory.Object);

            // act
            string actualAuthToken = await _sut.GetAuthTokenAsync(userId).ConfigureAwait(false);

            // assert
            Assert.AreEqual(authToken, actualAuthToken);
        }

        [Test]
        public void ItShouldRethrowExceptionWhenClientFactoryCallFails()
        {
            _oAuth2ClientFactory.Setup(x => x.GetOauth2ClientAsync(It.IsAny<int>())).Throws<Exception>();

            // act
            Func<Task> action = async () => await _sut.GetAuthTokenAsync(0).ConfigureAwait(false);

            // assert
            action.Should().Throw<Exception>();
        }

        [Test]
        public void ItShouldRethrowExceptionWhenTokenProviderCallFails()
        {
            const int userId = 1;
            Services.Security.Models.OAuth2Client client = new Services.Security.Models.OAuth2Client
            {
                Id = "id",
                Secret = "secret"
            };

            _oAuth2ClientFactory.Setup(x => x.GetOauth2ClientAsync(userId)).ReturnsAsync(client);

            Mock<ITokenProviderFactory> tokenProviderFactory = new Mock<ITokenProviderFactory>();
            Mock<ITokenProvider> tokenProvider = new Mock<ITokenProvider>();
            tokenProvider.Setup(x => x.GetAccessTokenAsync()).Throws<Exception>();
            tokenProviderFactory.Setup(x => x.GetTokenProvider(It.IsAny<string>(), It.IsAny<List<string>>())).Returns(tokenProvider.Object);
            _tokenProviderFactoryFactory.Setup(x => x.Create(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>())).Returns(tokenProviderFactory.Object);

            // act
            Func<Task> action = async () => await _sut.GetAuthTokenAsync(userId).ConfigureAwait(false);

            // assert
            action.Should().Throw<Exception>();
        }

        [Test]
        public void ItShouldRethrowExceptionWhenTokenProviderFactoryCallFails()
        {
            const string authToken = "auth_token";
            const int userId = 1;
            Services.Security.Models.OAuth2Client client = new Services.Security.Models.OAuth2Client
            {
                Id = "id",
                Secret = "secret"
            };

            _oAuth2ClientFactory.Setup(x => x.GetOauth2ClientAsync(userId)).ReturnsAsync(client);

            Mock<ITokenProviderFactory> tokenProviderFactory = new Mock<ITokenProviderFactory>();
            Mock<ITokenProvider> tokenProvider = new Mock<ITokenProvider>();
            tokenProvider.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync(authToken);
            tokenProviderFactory.Setup(x => x.GetTokenProvider(It.IsAny<string>(), It.IsAny<List<string>>())).Throws<Exception>();
            _tokenProviderFactoryFactory.Setup(x => x.Create(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>())).Returns(tokenProviderFactory.Object);

            // act
            Func<Task> action = async () => await _sut.GetAuthTokenAsync(userId).ConfigureAwait(false);

            // assert
            action.Should().Throw<Exception>();
        }

        [Test]
        public void ItShouldRethrowExceptionWhenTokenProviderFactoryFactoryCallFails()
        {
            const string authToken = "auth_token";
            const int userId = 1;
            Services.Security.Models.OAuth2Client client = new Services.Security.Models.OAuth2Client
            {
                Id = "id",
                Secret = "secret"
            };

            _oAuth2ClientFactory.Setup(x => x.GetOauth2ClientAsync(userId)).ReturnsAsync(client);

            Mock<ITokenProviderFactory> tokenProviderFactory = new Mock<ITokenProviderFactory>();
            Mock<ITokenProvider> tokenProvider = new Mock<ITokenProvider>();
            tokenProvider.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync(authToken);
            tokenProviderFactory.Setup(x => x.GetTokenProvider(It.IsAny<string>(), It.IsAny<List<string>>())).Returns(tokenProvider.Object);
            _tokenProviderFactoryFactory.Setup(x => x.Create(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>())).Throws<Exception>();

            // act
            Func<Task> action = async () => await _sut.GetAuthTokenAsync(userId).ConfigureAwait(false);

            // assert
            action.Should().Throw<Exception>();
        }
    }
}