using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Domain;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Security.Models;
using ITokenProvider = Relativity.OAuth2Client.Interfaces.ITokenProvider;

namespace kCura.IntegrationPoints.Core.Tests.Authentication
{
    [TestFixture, Category("Unit")]
    public class OAuth2TokenGeneratorTests : TestBase
    {
        private const string _CLIENTSECRETSTRING = "ClientSecret";
        private const string _CLIENTID = "ClientId";
        private OAuth2TokenGenerator _instance;
        private IHelper _helper;
        private IAPILog _logger;
        private IOAuth2ClientFactory _oAuth2ClientFactory;
        private ITokenProviderFactoryFactory _tokenProviderFactory;
        private ITokenProvider _tokenProvider;
        private IProvideServiceUris _uriProvider;
        private CurrentUser _currentUser;

        [SetUp]
        public override void SetUp()
        {
            var uri = new Uri("http://hostname");

            _logger = Substitute.For<IAPILog>();
            _helper = Substitute.For<IHelper>();
            _helper.GetLoggerFactory().GetLogger().ForContext<OAuth2TokenGenerator>().Returns(_logger);
            _helper.GetServicesManager().GetServicesURL().Returns(uri);
            _oAuth2ClientFactory = Substitute.For<IOAuth2ClientFactory>();
            _tokenProvider = Substitute.For<ITokenProvider>();
            _tokenProviderFactory = Substitute.For<ITokenProviderFactoryFactory>();
            _currentUser = new CurrentUser(userID: 1234);
            _uriProvider = Substitute.For<IProvideServiceUris>();
            _uriProvider.AuthenticationUri().Returns(uri);
            ExtensionPointServiceFinder.ServiceUriProvider = _uriProvider;

            _instance = new OAuth2TokenGenerator(_helper, _oAuth2ClientFactory, _tokenProviderFactory, _currentUser);
        }

        [Test]
        public void ItShouldGetAuthToken()
        {
            // ARRANGE
            var expectedToken = "ExpectedTokenString_1234";
            _tokenProvider.GetAccessTokenAsync().Returns(expectedToken);
            _oAuth2ClientFactory.GetOauth2ClientAsync(_currentUser.ID)
                .Returns(new OAuth2Client() { ContextUser = _currentUser.ID, Secret = _CLIENTSECRETSTRING, Id = _CLIENTID });
            _tokenProviderFactory.Create(Arg.Any<Uri>(), _CLIENTID, _CLIENTSECRETSTRING)
                .GetTokenProvider(Arg.Any<string>(), Arg.Any<IEnumerable<string>>()).Returns(_tokenProvider);

            // ACT
            string result = _instance.GetAuthToken();

            // ASSERT
            Assert.AreEqual(expectedToken, result);
        }

        [Test]
        public void ItShouldLogErrorWhenTokenGenerationFails()
        {
            // ARRANGE
            _oAuth2ClientFactory.GetOauth2ClientAsync(_currentUser.ID).Throws<Exception>();

            // ACT & ASSERT
            Assert.Throws<Exception>(() => _instance.GetAuthToken());
            _logger.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>());
        }
    }
}
