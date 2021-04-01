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
		
		public IntegrationPointTypeTest CreateImportType(WorkspaceTest workspace)
		{
			var integrationPointType = new IntegrationPointTypeTest
			{
				WorkspaceId = workspace.ArtifactId,
				Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ImportName,
				Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid.ToString(),
				ApplicationIdentifier = Const.INTEGRATION_POINTS_APP_GUID,
			};

			Database.IntegrationPointTypes.Add(integrationPointType);

			return integrationPointType;
		}

		public IntegrationPointTypeTest CreateExportType(WorkspaceTest workspace)
		{
			var integrationPointType = new IntegrationPointTypeTest
			{
				WorkspaceId = workspace.ArtifactId,
				Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportName,
				Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString(),
				ApplicationIdentifier = Const.INTEGRATION_POINTS_APP_GUID,
			};

			Database.IntegrationPointTypes.Add(integrationPointType);

			return integrationPointType;
		}
	}
}