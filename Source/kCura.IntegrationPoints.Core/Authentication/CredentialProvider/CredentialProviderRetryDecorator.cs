using System.Net;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;

namespace kCura.IntegrationPoints.Core.Authentication.CredentialProvider
{
	public class CredentialProviderRetryDecorator : ICredentialProvider
	{
		private readonly ICredentialProvider _credentialProvider;

		private readonly IRetryHandler _retryHandler;

		public CredentialProviderRetryDecorator(
			ICredentialProvider credentialProvider,
			IRetryHandlerFactory retryHandlerFactory)
		{
			_credentialProvider = credentialProvider;
			_retryHandler = retryHandlerFactory.Create();
		}

		public NetworkCredential Authenticate(CookieContainer cookieContainer)
		{
			return _retryHandler.ExecuteWithRetries(
				() => _credentialProvider.Authenticate(cookieContainer)
			);
		}
	}
}
