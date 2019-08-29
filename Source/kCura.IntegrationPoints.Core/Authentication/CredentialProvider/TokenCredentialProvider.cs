using System.Net;
using kCura.IntegrationPoints.Domain.Authentication;

namespace kCura.IntegrationPoints.Core.Authentication.CredentialProvider
{
	public class TokenCredentialProvider : ICredentialProvider
	{
		private readonly IAuthProvider _authProvider;
		private readonly IAuthTokenGenerator _tokenGenerator;

		public TokenCredentialProvider(IAuthProvider authProvider, IAuthTokenGenerator tokenGenerator)
		{
			_authProvider = authProvider;
			_tokenGenerator = tokenGenerator;
		}

		public NetworkCredential Authenticate(CookieContainer cookieContainer)
		{
			string token = _tokenGenerator.GetAuthToken();
			return _authProvider.LoginUsingAuthToken(token, cookieContainer);
		}
	}
}
