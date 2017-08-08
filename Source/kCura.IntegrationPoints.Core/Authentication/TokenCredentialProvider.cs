using System;
using System.Net;
using kCura.IntegrationPoints.Domain.Authentication;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Authentication
{
	public class TokenCredentialProvider : ICredentialProvider
	{
		private readonly IAuthProvider _authProvider;
		private readonly IAuthTokenGenerator _tokenGenerator;
		private readonly IAPILog _logger;

		public TokenCredentialProvider(IAuthProvider authProvider, IAuthTokenGenerator tokenGenerator, IHelper helper)
		{
			_authProvider = authProvider;
			_tokenGenerator = tokenGenerator;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<TokenCredentialProvider>();
		}

		public NetworkCredential Authenticate(CookieContainer cookieContainer)
		{
			try
			{
				string token = _tokenGenerator.GetAuthToken();
				return _authProvider.LoginUsingAuthToken(token, cookieContainer);
			}
			catch (Exception e)
			{
				_logger.LogError(e, $"Error occured while authenticating user. Details: {e.Message}");
				throw;
			}
		}
	}
}
