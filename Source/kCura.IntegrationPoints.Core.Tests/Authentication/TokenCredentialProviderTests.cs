using System.Net;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Core.Authentication.CredentialProvider;
using kCura.IntegrationPoints.Domain.Authentication;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Authentication
{
	public class TokenCredentialProviderTests : TestBase
	{
		private TokenCredentialProvider _instanceUnderTest;
		private IAuthProvider _authProvider;
		private IAuthTokenGenerator _tokenGenerator;
		private CookieContainer _cookieContainer;
		private string _fakeAuthToken = "AUTH_TOKEN";

		[SetUp]
		public override void SetUp()
		{
			_authProvider = Substitute.For<IAuthProvider>();
			_tokenGenerator = Substitute.For<IAuthTokenGenerator>();
			_tokenGenerator.GetAuthToken().Returns(_fakeAuthToken);
			_cookieContainer = new CookieContainer();

			_instanceUnderTest = new TokenCredentialProvider(_authProvider, _tokenGenerator);
		}

		[Test]
		public void ItShould_Authenticate()
		{
			_instanceUnderTest.Authenticate(_cookieContainer);

			_authProvider.Received(1).LoginUsingAuthToken(_fakeAuthToken, _cookieContainer);
		}
	}
}