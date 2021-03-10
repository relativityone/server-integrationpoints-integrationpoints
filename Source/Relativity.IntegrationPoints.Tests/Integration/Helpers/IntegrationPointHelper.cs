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
				ArtifactId = Artifact.NextId(),
				WorkspaceId = workspace.ArtifactId
			};

			Database.IntegrationPoints.Add(integrationPoint);

			return integrationPoint;
		}

		public void RemoveIntegrationPoint(int integrationPointId)
		{
			foreach (var integrationPoint in Database.IntegrationPoints.Where(x => x.ArtifactId == integrationPointId).ToArray())
			{
				Database.IntegrationPoints.Remove(integrationPoint);
			}
		}
	}
}
