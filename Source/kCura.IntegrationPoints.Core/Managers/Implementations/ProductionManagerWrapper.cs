using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Toggles;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    public class ProductionManagerWrapper : IProductionManagerWrapper
    {
        private readonly IProductionManagerWrapper _productionManager;
        
        public ProductionManagerWrapper(IToggleProvider toggleProvider, IProductionRepository productionRepository,
            IServiceManagerProvider serviceManagerProvider, IAPILog logger)
        {
            _productionManager = toggleProvider.IsEnabled<EnableKeplerizedImportAPIToggle>()
                ? (IProductionManagerWrapper) new KeplerProductManager(productionRepository) : new WebAPIProductionManager(serviceManagerProvider, logger);
        }
        public IEnumerable<ProductionDTO> GetProductionsForExport(int workspaceArtifactId)
        {
            return _productionManager.GetProductionsForExport(workspaceArtifactId);
        }

        public IEnumerable<ProductionDTO> GetProductionsForImport(int workspaceArtifactId, int? federatedInstanceId = null,
            string federatedInstanceCredentials = null)
        {
            return _productionManager.GetProductionsForImport(workspaceArtifactId, federatedInstanceId,
                federatedInstanceCredentials);
        }

        public bool IsProductionInDestinationWorkspaceAvailable(int workspaceArtifactId, int productionId,
            int? federatedInstanceId = null, string federatedInstanceCredentials = null)
        {
            return _productionManager.IsProductionInDestinationWorkspaceAvailable(workspaceArtifactId, productionId,
                federatedInstanceId, federatedInstanceCredentials);
        }
    }
}
