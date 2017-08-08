using System;
using System.Net;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Domain.Authentication;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Authentication
{
	public class TokenCredentialProviderTests : TestBase
	{
		private TokenCredentialProvider _instanceUnderTest;
		private IAuthProvider _authProvider;
		private IAuthTokenGenerator _tokenGenerator;
		private IHelper _helper;
		private IAPILog _logger;
		private CookieContainer _cookieContainer;
		private string _fakeAuthToken = "AUTH_TOKEN";

		[SetUp]
		public override void SetUp()
		{
			_authProvider = Substitute.For<IAuthProvider>();
			_tokenGenerator = Substitute.For<IAuthTokenGenerator>();
			_tokenGenerator.GetAuthToken().Returns(_fakeAuthToken);
			_logger = Substitute.For<IAPILog>();
			_helper = Substitute.For<IHelper>();
			_helper.GetLoggerFactory().GetLogger().ForContext<TokenCredentialProvider>().Returns(_logger);
			
			_cookieContainer = new CookieContainer();

			_instanceUnderTest = new TokenCredentialProvider(_authProvider, _tokenGenerator, _helper);
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
				//IGNORE
			}

			_logger.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>());
		}
	}
}