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

		public DestinationProviderTest CreateDestinationProvider(WorkspaceTest workspace)
		{
			var destinationProvider = new DestinationProviderTest()
			{
				WorkspaceId = workspace.ArtifactId
			};
			Database.DestinationProviders.Add(destinationProvider);
			return destinationProvider;
		}
	}
}