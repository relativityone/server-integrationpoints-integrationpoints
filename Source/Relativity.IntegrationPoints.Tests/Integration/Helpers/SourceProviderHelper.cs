using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class SourceProviderHelper : HelperBase
	{
		public SourceProviderHelper(HelperManager helperManager, InMemoryDatabase database, ProxyMock proxyMock)
			: base(helperManager, database, proxyMock)
		{
		}

		public SourceProviderTest CreateSourceProvider(WorkspaceTest workspace)
		{
			var sourceProvider = new SourceProviderTest()
			{
				WorkspaceId = workspace.ArtifactId
			};
			Database.SourceProviders.Add(sourceProvider);
			return sourceProvider;
		}
	}
}