using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Productions.Services;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class ProductionRepository : IProductionRepository
	{
		private readonly IServicesMgr _servicesMgr;

		public ProductionRepository(IServicesMgr servicesMgr)
		{
			_servicesMgr = servicesMgr;
		}

        public async Task<IEnumerable<ProductionDTO>> GetProductionsForExportAsync(int workspaceArtifactId)
        {
            IEnumerable<ProductionDTO> productionDtos;
			using (var productionService = _servicesMgr.CreateProxy<IProductionService>(ExecutionIdentity.CurrentUser))
            {
                productionDtos = await GetProductionsAsync(async (i, s) => 
                await productionService.RetrieveProducedByContextArtifactIDAsync(workspaceArtifactId, String.Empty), workspaceArtifactId)
                    .ConfigureAwait(false);
            }

            return productionDtos;
        }

        public async Task<IEnumerable<ProductionDTO>> GetProductionsForImportAsync(int workspaceArtifactId)
        {
            IEnumerable<ProductionDTO> productionDtos;
            using (var productionService = _servicesMgr.CreateProxy<IProductionService>(ExecutionIdentity.CurrentUser))
            {
                productionDtos = await GetProductionsAsync(async (i, s) => 
                        await productionService.RetrieveImportEligibleByContextArtifactIDAsync(workspaceArtifactId, String.Empty), workspaceArtifactId)
                    .ConfigureAwait(false);
            }

            return productionDtos;
        }

        public ProductionDTO GetProduction(int workspaceArtifactId, int productionArtifactId)
		{
			ProductionDTO productionDto;
			using (var productionManager = _servicesMgr.CreateProxy<IProductionManager>(ExecutionIdentity.CurrentUser))
			{
				try { 
					Production production = productionManager.ReadSingleAsync(workspaceArtifactId, productionArtifactId).Result;

					productionDto = new ProductionDTO
					{
						ArtifactID = production.ArtifactID.ToString(),
						DisplayName = production.Name
					};
				}
				catch (Exception ex)
				{
					throw new Exception("Unable to retrieve production", ex);
				}
			}

			return productionDto;
		}

		public int CreateSingle(int workspaceArtifactId, Production production)
		{
			int result;
			using (var productionManager = _servicesMgr.CreateProxy<IProductionManager>(ExecutionIdentity.CurrentUser))
			{
				try
				{
					result = productionManager.CreateSingleAsync(workspaceArtifactId, production).Result;
				}
				catch (Exception ex)
				{
					throw new Exception("Unable to create production", ex);
				}
			}
			return result;
		}

        private async Task<IEnumerable<ProductionDTO>> GetProductionsAsync(Func<int, string, Task<DataSetWrapper>> function, int workspaceArtifactId)
        {
            IEnumerable<ProductionDTO> productionDtos;

            try
            {
                DataSetWrapper dataSetWrapper = await function(workspaceArtifactId, String.Empty).ConfigureAwait(false);

                DataSet dataSet = dataSetWrapper.Unwrap();

                if (dataSet != null && dataSet.Tables.Count > 0)
                {
                    productionDtos = dataSet.Tables[0].AsEnumerable().Select(item => new ProductionDTO
                    {
                        ArtifactID = item.Field<int>("ArtifactID").ToString(),
                        DisplayName = item.Field<string>("Name"),
                    }
                    );
                }
                else
                {
                    throw new Exception($"No result returned when call to {nameof(function)} method!");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to retrieve productions for workspaceId: {workspaceArtifactId}", ex);
            }
            return productionDtos;
        }
    }
}
