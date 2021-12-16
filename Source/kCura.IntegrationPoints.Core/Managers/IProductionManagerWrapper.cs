using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
    public interface IProductionManagerWrapper
    {
        IEnumerable<ProductionDTO> GetProductionsForExport(int workspaceArtifactId);

        IEnumerable<ProductionDTO> GetProductionsForImport(int workspaceArtifactId, int? federatedInstanceId = null,
            string federatedInstanceCredentials = null);

        bool IsProductionInDestinationWorkspaceAvailable(int workspaceArtifactId, int productionId,
            int? federatedInstanceId = null, string federatedInstanceCredentials = null);
    }
}
