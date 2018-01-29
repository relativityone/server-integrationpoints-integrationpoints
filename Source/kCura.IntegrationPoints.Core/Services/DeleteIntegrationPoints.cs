using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Services
{
	public class DeleteIntegrationPoints
	{
		private readonly IIntegrationPointQuery _integrationPointQuery;
		private readonly IDeleteHistoryService _deleteHistoryService;
		private readonly IRSAPIService _service;

		public DeleteIntegrationPoints(IIntegrationPointQuery integrationPointQuery, IDeleteHistoryService deleteHistoryService, IRSAPIService service)
		{
			_integrationPointQuery = integrationPointQuery;
			_deleteHistoryService = deleteHistoryService;
			_service = service;
		}

		public void DeleteIPsWithSourceProvider(List<int> sourceProvider)
		{
			var integrationPoints = _integrationPointQuery.GetIntegrationPoints(sourceProvider);
			_deleteHistoryService.DeleteHistoriesAssociatedWithIPs(integrationPoints.Select(x => x.ArtifactId).ToList(), _service);
			foreach (var ip in integrationPoints)
			{
				_service.RelativityObjectManager.Delete(ip);
			}
		}
	}
}
