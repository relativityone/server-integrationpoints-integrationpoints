using System;
using System.Net;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Authentication.WebApi;
using kCura.IntegrationPoints.Core.Authentication.WebApi.LoginHelperFacade;
using kCura.IntegrationPoints.Domain.Authentication;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Authentication.WebApi
{
    [TestFixture, Category("Unit")]
    public class WebApiLoginServiceTests : TestBase
    {
        private WebApiLoginService _instanceUnderTest;
        private ILoginHelperFacade _authProvider;
        private IAuthTokenGenerator _tokenGenerator;
        private IAPILog _logger;
        private CookieContainer _cookieContainer;
        private string _fakeAuthToken = "AUTH_TOKEN";

        [SetUp]
        public override void SetUp()
        {
            _authProvider = Substitute.For<ILoginHelperFacade>();
            _tokenGenerator = Substitute.For<IAuthTokenGenerator>();
            _tokenGenerator.GetAuthToken().Returns(_fakeAuthToken);
            _logger = Substitute.For<IAPILog>();
            _cookieContainer = new CookieContainer();

            _instanceUnderTest = new WebApiLoginService(_authProvider, _tokenGenerator, _logger);
        }

        [Test]
        public void ItShould_Authenticate()
        {
            _instanceUnderTest.Authenticate(_cookieContainer);

            _authProvider.Received(1).LoginUsingAuthToken(_fakeAuthToken, _cookieContainer);
        }

        [Test]
        public void ItShould_LogError()
        {
            _authProvider.When(x => x.LoginUsingAuthToken(Arg.Any<string>(), Arg.Any<CookieContainer>())).Throw<Exception>();

            try
            {
                _instanceUnderTest.Authenticate(_cookieContainer);
            }
            catch
            {
                // IGNORE
            }

            _logger.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>());
        }
    }
}
