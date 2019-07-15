using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Data.Repositories;

namespace Rip.TestUtilities
{
	public class ProductionHelper
	{
		private readonly int _workspaceID;
		private readonly IRelativityObjectManager _relativityObjectManager;
		private readonly WorkspaceService _workspaceService;

		public ProductionHelper(
			int workspaceID,
			IRelativityObjectManager relativityObjectManager,
			WorkspaceService workspaceService)
		{
			_workspaceID = workspaceID;
			_relativityObjectManager = relativityObjectManager;
			_workspaceService = workspaceService;
		}

		public ProductionCreateResultDto CreateAndRunProduction(int searchArtifactID)
		{
			string productionName = Guid.NewGuid().ToString();
			return _workspaceService.CreateAndRunProduction(_workspaceID, searchArtifactID, productionName);
		}

		public void DeleteProduction(ProductionCreateResultDto productionCreateResult)
		{
			_relativityObjectManager.Delete(productionCreateResult.ProductionArtifactID);
			_relativityObjectManager.Delete(productionCreateResult.ProductionDataSourceArtifactID);
		}
	}
}
