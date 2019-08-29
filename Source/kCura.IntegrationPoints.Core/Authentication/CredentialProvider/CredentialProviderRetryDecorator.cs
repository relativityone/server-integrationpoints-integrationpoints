using System;
using System.Net;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Authentication.CredentialProvider
{
	public class CredentialProviderRetryDecorator : ICredentialProvider
	{
		private readonly ICredentialProvider _credentialProvider;

		private readonly IAPILog _logger;

		public CredentialProviderRetryDecorator(
			ICredentialProvider credentialProvider,
			IAPILog logger)
		{
			_credentialProvider = credentialProvider;
			_logger = logger;
		}

		public NetworkCredential Authenticate(CookieContainer cookieContainer)
		{
			// TODO implement retries
			try
			{
				return _credentialProvider.Authenticate(cookieContainer);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error occured while authenticating user. Details: {Message}", e.Message);
				throw;
			}
		}
	}
}
