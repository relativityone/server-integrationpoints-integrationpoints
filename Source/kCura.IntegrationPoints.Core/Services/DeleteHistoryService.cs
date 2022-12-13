using kCura.IntegrationPoints.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Core.Services
{
    public class DeleteHistoryService : IDeleteHistoryService
    {
        private readonly IIntegrationPointRepository _integrationPointRepository;

        public DeleteHistoryService(IIntegrationPointRepository integrationPointRepository)
        {
            _integrationPointRepository = integrationPointRepository;
        }

        public void DeleteHistoriesAssociatedWithIP(int integrationPointID)
        {
            DeleteHistoriesAssociatedWithIPs(new List<int> { integrationPointID });
        }

        public void DeleteHistoriesAssociatedWithIPs(List<int> integrationPointsIDs)
        {
            ValidateParameterIsNotNull(integrationPointsIDs, nameof(integrationPointsIDs));
            if (!integrationPointsIDs.Any())
            {
                return;
            }

            // Since 9.4 release we're not deleting job history RDOs (they've being used by ECA Dashboard)
            // We're also not removing JobHistoryErrors as it was taking too long (SQL timeouts)
            // JobHistoryErrors will be now removed by Management Agent

            foreach (int integrationPointId in integrationPointsIDs)
            {
                _integrationPointRepository.UpdateJobHistory(integrationPointId, null);
            }
        }

        private void ValidateParameterIsNotNull(object parameterValue, string parameterName)
        {
            if (parameterValue == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}
