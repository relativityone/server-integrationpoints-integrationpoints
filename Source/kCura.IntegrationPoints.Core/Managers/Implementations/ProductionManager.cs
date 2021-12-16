using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Productions.Services;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class ProductionManager : IProductionManager
	{
		private readonly IAPILog _logger;
		private readonly IRepositoryFactory _repositoryFactory;
        private readonly IProductionManagerWrapper _productionManagerWrapper;
        
		public ProductionManager(
			IAPILog logger, 
			IRepositoryFactory repositoryFactory,
            IProductionManagerWrapper productionManagerWrapper)
		{
			_logger = logger?.ForContext<ProductionManager>();
			_repositoryFactory = repositoryFactory;

            _productionManagerWrapper = productionManagerWrapper;
        }

		public ProductionDTO RetrieveProduction(int workspaceArtifactId, int productionArtifactId)
		{
			IProductionRepository productionRepository = _repositoryFactory.GetProductionRepository(workspaceArtifactId);
			return productionRepository.RetrieveProduction(workspaceArtifactId, productionArtifactId);
		}

        public int CreateSingle(int workspaceArtifactId, Production production)
		{
			IProductionRepository productionRepository = _repositoryFactory.GetProductionRepository(workspaceArtifactId);
			return productionRepository.CreateSingle(workspaceArtifactId, production);
		}

		public IEnumerable<ProductionDTO> GetProductionsForExport(int workspaceArtifactId)
        {
            return _productionManagerWrapper.GetProductionsForExport(workspaceArtifactId);
        }

		public IEnumerable<ProductionDTO> GetProductionsForImport(int workspaceArtifactId, int? federatedInstanceId = null, string federatedInstanceCredentials = null)
		{
            return _productionManagerWrapper.GetProductionsForImport(workspaceArtifactId, federatedInstanceId, federatedInstanceCredentials);
		}

		public bool IsProductionInDestinationWorkspaceAvailable(int workspaceArtifactId, int productionId, int? federatedInstanceId = null,
			string federatedInstanceCredentials = null)
		{
            return _productionManagerWrapper.IsProductionInDestinationWorkspaceAvailable(workspaceArtifactId, productionId, federatedInstanceId, federatedInstanceCredentials);
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
