using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.QueryBuilders;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Services.JobHistory
{
    public class CompletedJobsHistoryRepository : ICompletedJobsHistoryRepository
    {
        private readonly ICaseServiceContext _caseServiceContext;
        private readonly IIntegrationPointsCompletedJobsQueryBuilder _integrationPointsCompletedJobsQueryBuilder;

        public CompletedJobsHistoryRepository(ICaseServiceContext caseServiceContext, IIntegrationPointsCompletedJobsQueryBuilder integrationPointsCompletedJobsQueryBuilder)
        {
            _caseServiceContext = caseServiceContext;
            _integrationPointsCompletedJobsQueryBuilder = integrationPointsCompletedJobsQueryBuilder;
        }

        public IList<JobHistoryModel> RetrieveCompleteJobsForIntegrationPoints(JobHistoryRequest request, List<int> integrationPointIds)
        {
            bool sortDescending = (request.SortDescending != null) && request.SortDescending.Value;
            QueryRequest queryRequest = _integrationPointsCompletedJobsQueryBuilder.CreateQuery(request.SortColumnName, sortDescending, integrationPointIds);

            IList<kCura.IntegrationPoints.Data.JobHistory> queryResult =
                _caseServiceContext.RelativityObjectManagerService.RelativityObjectManager
                    .Query<kCura.IntegrationPoints.Data.JobHistory>(queryRequest);
            
            return queryResult.Select(x => new JobHistoryModel
            {
                ItemsTransferred = x.ItemsTransferred ?? 0,
                DestinationWorkspace = x.DestinationWorkspace,
                DestinationInstance = x.DestinationInstance,
                EndTimeUTC = x.EndTimeUTC.GetValueOrDefault(),
                FilesSize =  x.FilesSize, 
                Overwrite = x.Overwrite
            }).ToList();
        }

        public IList<JobHistoryModel> RetrieveCompleteJobsForIntegrationPoint(JobHistoryRequest request, int integrationPointId)
        {
            return RetrieveCompleteJobsForIntegrationPoints(request, new List<int>() { integrationPointId });
        }
    }
}