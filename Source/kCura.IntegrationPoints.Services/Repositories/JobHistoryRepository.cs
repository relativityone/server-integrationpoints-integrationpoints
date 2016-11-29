using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.JobHistory;
using kCura.Relativity.Client.DTOs;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services.Repositories
{
	public class JobHistoryRepository : IJobHistoryRepository
	{
		private readonly ILog _logger;
		private readonly ICompletedJobQueryBuilder _completedJobQueryBuilder;
		private readonly IWorkspaceManager _workspaceManager;
		private readonly IJobHistoryAccess _jobHistoryAccess;
		private readonly IJobHistorySummaryModelBuilder _summaryModelBuilder;
		private readonly IJobHistoryLibraryFactory _jobHistoryLibraryFactory;

		public JobHistoryRepository(ILog logger, ICompletedJobQueryBuilder completedJobQueryBuilder, IWorkspaceManager workspaceManager, IJobHistoryAccess jobHistoryAccess,
			IJobHistorySummaryModelBuilder summaryModelBuilder, IJobHistoryLibraryFactory jobHistoryLibraryFactory)
		{
			_logger = logger;
			_completedJobQueryBuilder = completedJobQueryBuilder;
			_workspaceManager = workspaceManager;
			_jobHistoryAccess = jobHistoryAccess;
			_summaryModelBuilder = summaryModelBuilder;
			_jobHistoryLibraryFactory = jobHistoryLibraryFactory;
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

				Query<RDO> query = _completedJobQueryBuilder.CreateQuery(request.SortColumnName, (request.SortDescending != null) && request.SortDescending.Value);

				IGenericLibrary<Data.JobHistory> library = _jobHistoryLibraryFactory.Create(request.WorkspaceArtifactId);
				IList<Data.JobHistory> queryResult = library.Query(query);

				IList<Data.JobHistory> jobHistories = _jobHistoryAccess.Filter(queryResult, workspacesWithAccess);

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