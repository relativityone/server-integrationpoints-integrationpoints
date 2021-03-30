using System.Linq;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class IntegrationPointHelper : HelperBase
	{
		public IntegrationPointHelper(HelperManager manager, InMemoryDatabase database, ProxyMock proxyMock) 
			: base(manager, database, proxyMock)
		{
		}

		public IntegrationPointTest CreateEmptyIntegrationPoint(WorkspaceTest workspace)
		{
			var integrationPoint = new IntegrationPointTest
			{
				WorkspaceId = workspace.ArtifactId
			};

			Database.IntegrationPoints.Add(integrationPoint);

			return integrationPoint;
		}

		public IntegrationPointTest CreateIntegrationPointWithFakeProviders(WorkspaceTest workspace)
		{
			SourceProviderTest sourceProvider = HelperManager.SourceProviderHelper.CreateSourceProvider(workspace);
			DestinationProviderTest destinationProviderTest = HelperManager.DestinationProviderHelper.CreateDestinationProvider(workspace);
			IntegrationPointTypeTest integrationPointType = HelperManager.IntegrationPointTypeHelper.CreateIntegrationPointType(workspace);

			IntegrationPointTest integrationPoint = CreateEmptyIntegrationPoint(workspace);

			integrationPoint.Type = integrationPointType.ArtifactId;
			integrationPoint.SourceProvider = sourceProvider.ArtifactId;
			integrationPoint.DestinationProvider = destinationProviderTest.ArtifactId;

			return integrationPoint;
		}

		public IntegrationPointTest CreateIntegrationPoint(IntegrationPointTest integrationPoint)
		{
			Database.IntegrationPoints.Add(integrationPoint);

			return integrationPoint;
		}

		public void RemoveIntegrationPoint(int integrationPointId)
		{
			foreach (IntegrationPointTest integrationPoint in Database.IntegrationPoints.Where(x => x.ArtifactId == integrationPointId).ToArray())
			{
				Database.IntegrationPoints.Remove(integrationPoint);
			}
		}
	}
}
