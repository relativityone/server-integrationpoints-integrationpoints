using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.RSAPIClient;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
	public class ExtendedRsapiClientWithWorkspaceFactory : RsapiClientWithWorkspaceFactory
	{
		private readonly IAuthTokenGenerator _tokenGenerator;

		public ExtendedRsapiClientWithWorkspaceFactory(IHelper helper, IAuthTokenGenerator tokenGenerator)
			: base(helper)
		{
			_tokenGenerator = tokenGenerator;
		}

		public override IRSAPIClient CreateUserClient(int workspaceArtifactId)
		{
			string token = _tokenGenerator.GetAuthToken();
			IRSAPIClient rsapiClient = new RSAPIClient(Helper.GetServicesManager().GetServicesURL(), new BearerTokenCredentials(token));
			rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

			IAPILog logger = Helper.GetLoggerFactory().GetLogger();
			return new RsapiClientWrapperWithLogging(rsapiClient, logger);
		}
	}
}