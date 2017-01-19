using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Toggles;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.JobHistory;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Services.Repositories.Implementations
{
	public class JobHistoryRepository : IJobHistoryRepository
	{
		private readonly IRelativityIntegrationPointsRepository _relativityIntegrationPointsRepository;
		private readonly ICompletedJobsHistoryRepository _completedJobsHistoryRepository;
		private readonly IWorkspaceManager _workspaceManager;
		private readonly IJobHistoryAccess _jobHistoryAccess;
		private readonly IJobHistorySummaryModelBuilder _summaryModelBuilder;
		private readonly IToggleProvider _toggleProvider;

		public JobHistoryRepository(IRelativityIntegrationPointsRepository relativityIntegrationPointsRepository,
			ICompletedJobsHistoryRepository completedJobsHistoryRepository, IWorkspaceManager workspaceManager, IJobHistoryAccess jobHistoryAccess,
			IJobHistorySummaryModelBuilder summaryModelBuilder, IToggleProvider toggleProvider)
		{
			_relativityIntegrationPointsRepository = relativityIntegrationPointsRepository;
			_completedJobsHistoryRepository = completedJobsHistoryRepository;
			_workspaceManager = workspaceManager;
			_jobHistoryAccess = jobHistoryAccess;
			_summaryModelBuilder = summaryModelBuilder;
			_toggleProvider = toggleProvider;
		}

		public JobHistorySummaryModel GetJobHistory(JobHistoryRequest request)
		{
			IList<int> workspacesWithAccess = new List<int>();
			if (!_toggleProvider.IsEnabled<RipToR1Toggle>())
			{
				workspacesWithAccess = _workspaceManager.GetIdsOfWorkspacesUserHasPermissionToView();
				if (!workspacesWithAccess.Any())
				{
					return new JobHistorySummaryModel();
				}
			}

			List<int> integrationPointIds = _relativityIntegrationPointsRepository.RetrieveRelativityIntegrationPointsIds(request.WorkspaceArtifactId);

			IList<JobHistoryModel> queryResult = _completedJobsHistoryRepository.RetrieveCompleteJobsForIntegrationPoints(request, integrationPointIds);

			IList<JobHistoryModel> jobHistories = new List<JobHistoryModel>();
			if (!_toggleProvider.IsEnabled<RipToR1Toggle>())
			{
				jobHistories = _jobHistoryAccess.Filter(queryResult, workspacesWithAccess);
			}

			return _summaryModelBuilder.Create(request.Page, request.PageSize, jobHistories);
		}
	}
}