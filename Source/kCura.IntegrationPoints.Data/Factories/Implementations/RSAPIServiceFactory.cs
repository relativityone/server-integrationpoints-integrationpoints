using Relativity.API;

namespace kCura.IntegrationPoints.Data.Factories.Implementations
{
	public class RSAPIServiceFactory : IRSAPIServiceFactory
	{
		private readonly IHelper _helper;

		public RSAPIServiceFactory(IHelper helper)
		{
			_helper = helper;
		}

		public IRSAPIService Create(int workspaceArtifactId)
		{
			return new RSAPIService(_helper, workspaceArtifactId);
		}

		public IRSAPIService CreateAdminAccess(int workspaceArtifactId)
		{
			return new RSAPIServiceAdminAccess(_helper, workspaceArtifactId);
		}
	}
}