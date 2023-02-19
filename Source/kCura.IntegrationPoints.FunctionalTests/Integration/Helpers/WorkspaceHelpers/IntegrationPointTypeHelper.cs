using Relativity.IntegrationPoints.Tests.Common;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public class IntegrationPointTypeHelper : WorkspaceHelperBase
    {
        public IntegrationPointTypeHelper(WorkspaceTest workspace) : base(workspace)
        {
        }

        public IntegrationPointTypeTest CreateImportType()
        {
            var integrationPointType = new IntegrationPointTypeTest
            {
                Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ImportName,
                Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid.ToString(),
                ApplicationIdentifier = GlobalConst.INTEGRATION_POINTS_APPLICATION_GUID,
            };

            Workspace.IntegrationPointTypes.Add(integrationPointType);

            return integrationPointType;
        }

        public IntegrationPointTypeTest CreateExportType()
        {
            var integrationPointType = new IntegrationPointTypeTest
            {
                Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportName,
                Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString(),
                ApplicationIdentifier = GlobalConst.INTEGRATION_POINTS_APPLICATION_GUID,
            };

            Workspace.IntegrationPointTypes.Add(integrationPointType);

            return integrationPointType;
        }
    }
}
