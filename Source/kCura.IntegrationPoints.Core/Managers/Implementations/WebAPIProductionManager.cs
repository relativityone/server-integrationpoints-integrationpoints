using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.EDDS.WebAPI.ProductionManagerBase;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    public class WebAPIProductionManager : IProductionManagerWrapper
    {
        private readonly IServiceManagerProvider _serviceManagerProvider;
        private readonly IAPILog _logger;

        public WebAPIProductionManager(IServiceManagerProvider serviceManagerProvider, IAPILog logger)
        {
            _serviceManagerProvider = serviceManagerProvider;
            _logger = logger?.ForContext<WebAPIProductionManager>();
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

        public IEnumerable<ProductionDTO> GetProductionsForImport(int workspaceArtifactId, int? federatedInstanceId = null,
            string federatedInstanceCredentials = null)
        {
            try
            {
                WinEDDS.Service.Export.IProductionManager productionManager =
                    _serviceManagerProvider.Create<WinEDDS.Service.Export.IProductionManager, ProductionManagerFactory>();
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

        public bool IsProductionInDestinationWorkspaceAvailable(int workspaceArtifactId, int productionId,
            int? federatedInstanceId = null, string federatedInstanceCredentials = null)
        {
            try
            {
                WinEDDS.Service.Export.IProductionManager productionManager =
                    _serviceManagerProvider.Create<WinEDDS.Service.Export.IProductionManager, ProductionManagerFactory>();
                ProductionInfo productionInfo = productionManager.Read(workspaceArtifactId, productionId);
                return productionInfo != null;
            }
            catch (Exception ex)
            {
                var parametersToLog = new { workspaceArtifactId, productionId, federatedInstanceId };
                _logger?.LogWarning(ex, "Error occured while verifying if production is available in destination workspace. Parameters: {@parametersToLog}", parametersToLog);
                return false;
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
