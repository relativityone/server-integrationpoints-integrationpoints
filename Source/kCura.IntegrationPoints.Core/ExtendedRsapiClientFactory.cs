using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
	public class ExtendedRsapiClientFactory : RsapiClientFactory 
	{
		private readonly IAuthTokenGenerator _tokenGenerator;

		public ExtendedRsapiClientFactory(IHelper helper, IAuthTokenGenerator tokenGenerator)
			: base(helper)
		{
			_tokenGenerator = tokenGenerator;
		}

		public override IRSAPIClient CreateUserClient(int workspaceArtifactId)
		{
			string token = _tokenGenerator.GetAuthToken();
			IRSAPIClient rsapiClient = new RSAPIClient(Helper.GetServicesManager().GetServicesURL(), new BearerTokenCredentials(token));
			rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

			return rsapiClient;
		}
	}
}