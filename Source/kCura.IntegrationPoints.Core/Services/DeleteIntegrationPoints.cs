using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Services
{
	public class DeleteIntegrationPoints
	{
		private readonly IntegrationPointQuery _integrationPointQuery ;
		private readonly DeleteHistoryService _deleteHistoryService;
		private readonly IRSAPIService _service;

		public DeleteIntegrationPoints(IntegrationPointQuery integrationPointQuery, DeleteHistoryService deleteHistoryService,IRSAPIService service)
		{
			_integrationPointQuery = integrationPointQuery;
			_deleteHistoryService = deleteHistoryService;
			_service = service;
		}

		public void DeleteIPsWithSourceProvider(List<int> sourceProvider)
		{
			
			var integrationPoint = _integrationPointQuery.GetIntegrationPoints(sourceProvider);
			_deleteHistoryService.DeleteHistoriesAssociatedWithIPs(integrationPoint.Select(x => x.ArtifactId).ToList(), _service);
			_service.IntegrationPointLibrary.Delete(integrationPoint);


		}
	}
}
