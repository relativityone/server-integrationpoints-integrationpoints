using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Domain.Authentication;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Authentication.CredentialProvider
{
	/// <summary>
	/// This factory can be used to retrieve instance of <see cref="ICredentialProvider"/> when resolving it from IoC container is not possible
	/// </summary>
	public class CredentialProviderFactoryDeprecated : ICredentialProviderFactoryDeprecated
	{
		private readonly IAPILog _logger;

		public CredentialProviderFactoryDeprecated(IAPILog logger)
		{
			_logger = logger.ForContext<CredentialProviderFactoryDeprecated>();
		}

		public ICredentialProvider Create()
		{
			ICredentialProvider tokenCredentialProvider = CreteTokenCredentialProvider();
			ICredentialProvider credentialProviderRetryDecorator = CreateCredentialProviderRetryDecorator(tokenCredentialProvider);
			return credentialProviderRetryDecorator;
		}

		private ICredentialProvider CreteTokenCredentialProvider()
		{
			var authProvider = new AuthProvider();
			var tokenGenerator = new ClaimsTokenGenerator();
			return new TokenCredentialProvider(authProvider, tokenGenerator);
		}

		private ICredentialProvider CreateCredentialProviderRetryDecorator(ICredentialProvider credentialProvider)
		{
			var retryHandlerFactory = new RetryHandlerFactory(_logger);
			return new CredentialProviderRetryDecorator(credentialProvider, retryHandlerFactory);
		}
	}
}
