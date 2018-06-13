﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
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

		public ProductionDTO RetrieveProduction(int workspaceArtifactId, int productionArtifactId)
		{
			ProductionDTO productionDto;
			using (var productionManager = _servicesMgr.CreateProxy<IProductionManager>(ExecutionIdentity.CurrentUser))
			{
				try { 
					Production production = productionManager.ReadSingleAsync(workspaceArtifactId, productionArtifactId).Result;

					productionDto = new ProductionDTO()
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
	}
}
