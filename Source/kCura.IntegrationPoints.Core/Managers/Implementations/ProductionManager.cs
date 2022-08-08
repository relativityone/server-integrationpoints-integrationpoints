using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Productions.Services;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    public class ProductionManager : IProductionManager
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IAPILog _logger;

        public ProductionManager(IRepositoryFactory repositoryFactory, IAPILog logger)
        {
            _repositoryFactory = repositoryFactory;
            _logger = logger?.ForContext<ProductionManager>();
        }

        public ProductionDTO RetrieveProduction(int workspaceArtifactId, int productionArtifactId)
        {
            IProductionRepository productionRepository = _repositoryFactory.GetProductionRepository(workspaceArtifactId);
            return productionRepository.GetProduction(workspaceArtifactId, productionArtifactId);
        }

        public int CreateSingle(int workspaceArtifactId, Production production)
        {
            IProductionRepository productionRepository = _repositoryFactory.GetProductionRepository(workspaceArtifactId);
            return productionRepository.CreateSingle(workspaceArtifactId, production);
        }

        public IEnumerable<ProductionDTO> GetProductionsForExport(int workspaceArtifactId)
        {
            IEnumerable<ProductionDTO> productions = Task.Run(() => _repositoryFactory.GetProductionRepository(workspaceArtifactId).GetProductionsForExport(workspaceArtifactId)).GetAwaiter().GetResult();
            return productions;
        }

        public IEnumerable<ProductionDTO> GetProductionsForImport(int workspaceArtifactId, int? federatedInstanceId = null, string federatedInstanceCredentials = null)
        {
            IEnumerable<ProductionDTO> productions = Task.Run(() => _repositoryFactory.GetProductionRepository(workspaceArtifactId).GetProductionsForImport(workspaceArtifactId)).GetAwaiter().GetResult();
            return productions;
        }

        public bool IsProductionInDestinationWorkspaceAvailable(int workspaceArtifactId, int productionId, int? federatedInstanceId = null,
            string federatedInstanceCredentials = null)
        {
            try
            {
                ProductionDTO production = _repositoryFactory.GetProductionRepository(workspaceArtifactId).GetProduction(workspaceArtifactId, productionId);
                return production != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve production Artifact ID {productionId} from workspace Artifact ID {workspaceArtifactId}", productionId, workspaceArtifactId);
                return false;
            }
        }

        public bool IsProductionEligibleForImport(int workspaceArtifactId, int productionId, int? federatedInstanceId = null,
            string federatedInstanceCredentials = null)
        {
            try
            {
                IEnumerable<ProductionDTO> allEligibleProductions = GetProductionsForImport(workspaceArtifactId, federatedInstanceId, federatedInstanceCredentials);
                ProductionDTO production = allEligibleProductions.FirstOrDefault(x => x.ArtifactID.Equals(productionId.ToString()));
                return production != null;
            }
            catch (Exception ex)
            {
                var parametersToLog = new { workspaceArtifactId, productionId, federatedInstanceId };
                _logger?.LogError(ex, "Error occured while verifying if production is eligible for import. Parameters: {@parametersToLog}", parametersToLog);
                return false;
            }
        }

    }
}
