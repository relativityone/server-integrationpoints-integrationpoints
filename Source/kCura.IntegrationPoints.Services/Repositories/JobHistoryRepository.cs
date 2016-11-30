using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.JobHistory;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services.Repositories
{
	public class JobHistoryRepository : IJobHistoryRepository
	{
		private readonly ILog _logger;
		private readonly IRelativityIntegrationPointsRepository _relativityIntegrationPointsRepository;
		private readonly ICompletedJobsHistoryRepository _completedJobsHistoryRepository;
		private readonly IWorkspaceManager _workspaceManager;
		private readonly IJobHistoryAccess _jobHistoryAccess;
		private readonly IJobHistorySummaryModelBuilder _summaryModelBuilder;

		public JobHistoryRepository(ILog logger, IRelativityIntegrationPointsRepository relativityIntegrationPointsRepository,
			ICompletedJobsHistoryRepository completedJobsHistoryRepository, IWorkspaceManager workspaceManager, IJobHistoryAccess jobHistoryAccess,
			IJobHistorySummaryModelBuilder summaryModelBuilder)
		{
			_logger = logger;
			_relativityIntegrationPointsRepository = relativityIntegrationPointsRepository;
			_completedJobsHistoryRepository = completedJobsHistoryRepository;
			_workspaceManager = workspaceManager;
			_jobHistoryAccess = jobHistoryAccess;
			_summaryModelBuilder = summaryModelBuilder;
		}

		public JobHistorySummaryModel GetJobHistory(JobHistoryRequest request)
		{
			try
			{
				IList<int> workspacesWithAccess = _workspaceManager.GetIdsOfWorkspacesUserHasPermissionToView();
				if (!workspacesWithAccess.Any())
				{
					return new JobHistorySummaryModel();
				}

				List<IntegrationPoint> integrationPoints = _relativityIntegrationPointsRepository.RetrieveRelativityIntegrationPoints(request.WorkspaceArtifactId);

				IList<JobHistoryModel> queryResult = _completedJobsHistoryRepository.RetrieveCompleteJobsForIntegrationPoints(request, integrationPoints);

				IList<JobHistoryModel> jobHistories = _jobHistoryAccess.Filter(queryResult, workspacesWithAccess);

				return _summaryModelBuilder.Create(request.Page, request.PageSize, jobHistories);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "{0}.{1}", nameof(JobHistoryManager), nameof(GetJobHistory));
				throw;
			}
		}
	}
}