using System;
using System.Net;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;
using kCura.IntegrationPoints.Domain.Authentication;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Authentication
{
	public class TokenCredentialProvider : ICredentialProvider
	{
		private readonly ushort _MAX_NUMBER_OF_RETRTIES = 3;
		private readonly ushort _EXPONENTIAL_WAIT_TIME_BASE_IN_SECONDS = 3;

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
				return LoginUsingAuthToken(cookieContainer, token);
			}
			catch (Exception e)
			{
				_logger.LogError(e, $"Error occured while authenticating user. Details: {e.Message}");
				throw;
			}
		}

		private NetworkCredential LoginUsingAuthToken(CookieContainer cookieContainer, string token)
		{
			var retryHandlerFactory = new RetryHandlerFactory(_logger);
			IRetryHandler retryHandler = retryHandlerFactory.Create(_MAX_NUMBER_OF_RETRTIES, _EXPONENTIAL_WAIT_TIME_BASE_IN_SECONDS);

			return retryHandler.ExecuteWithRetries(
				() => _authProvider.LoginUsingAuthToken(token, cookieContainer)
			);
		}
	}
}
