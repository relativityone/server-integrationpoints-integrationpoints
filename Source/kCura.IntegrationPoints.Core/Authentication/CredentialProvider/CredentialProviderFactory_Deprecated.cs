using kCura.IntegrationPoints.Domain.Authentication;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Authentication.CredentialProvider
{
	/// <summary>
	/// This factory can be used to retrieve instance of <see cref="ICredentialProvider"/> when resolving it from IoC container is not possible
	/// </summary>
	public class CredentialProviderFactory_Deprecated : ICredentialProviderFactory_Deprecated
	{
		private readonly IAPILog _logger;

		public CredentialProviderFactory_Deprecated(IAPILog logger)
		{
			_logger = logger.ForContext<CredentialProviderFactory_Deprecated>();
		}

		public ICredentialProvider Create()
		{
			IAuthProvider authProvider = new AuthProvider();
			IAuthTokenGenerator tokenGenerator = new ClaimsTokenGenerator();
			ICredentialProvider credentialProvider = new TokenCredentialProvider(authProvider, tokenGenerator);
			ICredentialProvider credentialProviderRetryDecorator = new CredentialProviderRetryDecorator(credentialProvider, _logger);
			return credentialProviderRetryDecorator;
		}
	}
}
