using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Productions.Services;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class ProductionManager : IProductionManager
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IServiceManagerProvider _serviceManagerProvider;
		private readonly IFederatedInstanceManager _federatedInstanceManager;

		internal ProductionManager(IRepositoryFactory repositoryFactory, IServiceManagerProvider serviceManagerProvider, IFederatedInstanceManager federatedInstanceManager)
		{
			_repositoryFactory = repositoryFactory;
			_serviceManagerProvider = serviceManagerProvider;
			_federatedInstanceManager = federatedInstanceManager;
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
			try
			{
				WinEDDS.Service.Export.IProductionManager productionManager =
					_serviceManagerProvider.Create<WinEDDS.Service.Export.IProductionManager, ProductionManagerFactory>();
				DataTable dt = productionManager.RetrieveProducedByContextArtifactID(workspaceArtifactId).Tables[0];

				return CreateProductionsCollection(dt);
			}
			catch (Exception ex)
			{
				string message = IntegrationPointsExceptionMessages.CreateErrorMessageRetryOrContactAdministrator("getting production for export");
				throw new IntegrationPointsException(message, ex)
				{
					ExceptionSource = IntegrationPointsExceptionSource.WIN_EDDS
				};
			}
		}

		public IEnumerable<ProductionDTO> GetProductionsForImport(int workspaceArtifactId, int? federatedInstanceId = null, string federatedInstanceCredentials = null)
		{
			try
			{
				WinEDDS.Service.Export.IProductionManager productionManager =
					_serviceManagerProvider.Create<WinEDDS.Service.Export.IProductionManager, ProductionManagerFactory>(federatedInstanceId, federatedInstanceCredentials, _federatedInstanceManager);
				DataTable dt = productionManager.RetrieveImportEligibleByContextArtifactID(workspaceArtifactId).Tables[0];

				return CreateProductionsCollection(dt);
			}
			catch (Exception ex)
			{
				string message = IntegrationPointsExceptionMessages.CreateErrorMessageRetryOrContactAdministrator("getting production for import");
				throw new IntegrationPointsException(message, ex)
				{
					ExceptionSource = IntegrationPointsExceptionSource.WIN_EDDS
				};
			}
		}

		private static IEnumerable<ProductionDTO> CreateProductionsCollection(DataTable table)
		{
			return (from DataRow row in table.Rows
					select new ProductionDTO
					{
						ArtifactID = row["ArtifactID"].ToString(),
						DisplayName = row["Name"].ToString()
					}).ToList();
		}
	}
}
