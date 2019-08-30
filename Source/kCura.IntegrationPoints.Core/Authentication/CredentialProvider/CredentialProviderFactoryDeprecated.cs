using kCura.IntegrationPoints.Core.Authentication.AuthProvider;
using kCura.IntegrationPoints.Domain.Authentication;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Authentication.CredentialProvider
{
	public static class CredentialProviderFactoryDeprecated
	{
		/// <summary>
		///  This method can be used to retrieve instance of <see cref="ICredentialProvider"/> when resolving it from IoC container is not possible
		/// </summary>
		/// <param name="logger"></param>
		/// <returns></returns>
		public static ICredentialProvider Create(IAPILog logger)
		{
			IAuthProvider authProvider = AuthProviderFactoryDeprecated.Create(logger);
			var tokenGenerator = new ClaimsTokenGenerator();
			return new TokenCredentialProvider(authProvider, tokenGenerator, logger);
		}
	}
}
