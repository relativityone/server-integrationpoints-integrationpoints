using System;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class IntegrationPointHelper : HelperBase
	{
		public IntegrationPointHelper(HelperManager manager, InMemoryDatabase database, ProxyMock proxyMock) : base(manager, database, proxyMock)
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

			ProxyMock.ObjectManager.SetupIntegrationPoint(workspace, integrationPoint);

			return integrationPoint;
		}

		public void RemoveIntegrationPoint(int integrationPointId)
		{
			Database.IntegrationPoints.RemoveAll(x => x.ArtifactId == integrationPointId);
		}
	}
}
