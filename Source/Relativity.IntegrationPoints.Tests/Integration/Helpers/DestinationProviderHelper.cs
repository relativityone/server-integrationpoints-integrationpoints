using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class DestinationProviderHelper : HelperBase
	{
		public DestinationProviderHelper(HelperManager helperManager, InMemoryDatabase database, ProxyMock proxyMock)
			: base(helperManager, database, proxyMock)
		{
		}

		public void CreateRelativity(WorkspaceTest workspace)
		{
			var destinationProvider = new DestinationProviderTest()
			{
				WorkspaceId = workspace.ArtifactId,
				ApplicationIdentifier = Const.INTEGRATION_POINTS_APP_GUID,
				Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY,
				Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY_NAME
			};
			
			Database.DestinationProviders.Add(destinationProvider);
		}

		public void CreateLoadFile(WorkspaceTest workspace)
		{
			var destinationProvider = new DestinationProviderTest()
			{
				WorkspaceId = workspace.ArtifactId,
				ApplicationIdentifier = Const.INTEGRATION_POINTS_APP_GUID,
				Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.LOADFILE,
				Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.LOADFILE_NAME
			};

			Database.DestinationProviders.Add(destinationProvider);
		}
	}
}