using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Provider.Internals
{
	public class IntegrationPointsRemover : IIntegrationPointsRemover
	{
		private readonly IIntegrationPointQuery _integrationPointQuery;
		private readonly IDeleteHistoryService _deleteHistoryService;
		private readonly IRelativityObjectManager _objectManager;

		public IntegrationPointsRemover(IIntegrationPointQuery integrationPointQuery, IDeleteHistoryService deleteHistoryService, IRelativityObjectManager objectManager)
		{
			_integrationPointQuery = integrationPointQuery;
			_deleteHistoryService = deleteHistoryService;
			_objectManager = objectManager;
		}

		public void DeleteIntegrationPointsBySourceProvider(List<int> sourceProvidersIds)
		{
			IList<IntegrationPoint> integrationPoints = _integrationPointQuery.GetIntegrationPoints(sourceProvidersIds);
			_deleteHistoryService.DeleteHistoriesAssociatedWithIPs(integrationPoints.Select(x => x.ArtifactId).ToList(), _objectManager);
			foreach (IntegrationPoint ip in integrationPoints)
			{
				_objectManager.Delete(ip);
			}
		}
	}
}
