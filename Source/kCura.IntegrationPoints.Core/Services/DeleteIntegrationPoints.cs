﻿using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Services
{
	public class DeleteIntegrationPoints
	{
		private readonly IIntegrationPointQuery _integrationPointQuery;
		private readonly IDeleteHistoryService _deleteHistoryService;
		private readonly IRelativityObjectManager _objectManager;

		public DeleteIntegrationPoints(IIntegrationPointQuery integrationPointQuery, IDeleteHistoryService deleteHistoryService, IRelativityObjectManager objectManager)
		{
			_integrationPointQuery = integrationPointQuery;
			_deleteHistoryService = deleteHistoryService;
			_objectManager = objectManager;
		}

		public void DeleteIPsWithSourceProvider(List<int> sourceProvider)
		{
			IList<Data.IntegrationPoint> integrationPoints = _integrationPointQuery.GetIntegrationPoints(sourceProvider);
			_deleteHistoryService.DeleteHistoriesAssociatedWithIPs(integrationPoints.Select(x => x.ArtifactId).ToList(), _objectManager);
			foreach (Data.IntegrationPoint ip in integrationPoints)
			{
				_objectManager.Delete(ip);
			}
		}
	}
}
