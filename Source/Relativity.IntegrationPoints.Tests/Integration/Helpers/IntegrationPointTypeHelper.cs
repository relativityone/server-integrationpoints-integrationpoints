using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class IntegrationPointTypeHelper : HelperBase
	{
		public IntegrationPointTypeHelper(HelperManager helperManager, InMemoryDatabase database, ProxyMock proxyMock)
			: base(helperManager, database, proxyMock)
		{
		}

		public IntegrationPointTypeTest CreateIntegrationPointType(WorkspaceTest workspace)
		{
			var integrationPointType = new IntegrationPointTypeTest
			{
				WorkspaceId = workspace.ArtifactId
			};

			Database.IntegrationPointTypes.Add(integrationPointType);

			return integrationPointType;
		}

		public IntegrationPointTypeTest CreateIntegrationPointType(IntegrationPointTypeTest integrationPointType)
		{
			Database.IntegrationPointTypes.Add(integrationPointType);

			return integrationPointType;
		}
	}
}