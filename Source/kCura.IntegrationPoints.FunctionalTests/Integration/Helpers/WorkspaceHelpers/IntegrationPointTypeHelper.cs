using Relativity.IntegrationPoints.Tests.Common;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public class IntegrationPointTypeHelper : WorkspaceHelperBase
    {
        public IntegrationPointTypeHelper(WorkspaceFake workspace) : base(workspace)
        {
        }

        public IntegrationPointTypeFake CreateImportType()
        {
            var integrationPointType = new IntegrationPointTypeFake
            {
                Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ImportName,
                Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid.ToString(),
                ApplicationIdentifier = GlobalConst.INTEGRATION_POINTS_APPLICATION_GUID,
            };

            Workspace.IntegrationPointTypes.Add(integrationPointType);

            return integrationPointType;
        }

        public IntegrationPointTypeFake CreateExportType()
        {
            var integrationPointType = new IntegrationPointTypeFake
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
