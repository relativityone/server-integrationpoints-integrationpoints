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
		private readonly ILibraryFactory _libraryFactory;
		private readonly IIntegrationPointByProvidersQueryBuilder _integrationPointByProvidersQueryBuilder = new IntegrationPointByProvidersQueryBuilder();
		private readonly SourceProviderArtifactIdByGuidQueryBuilder _sourceProviderArtifactIdByGuidQueryBuilder = new SourceProviderArtifactIdByGuidQueryBuilder();
		private readonly DestinationProviderArtifactIdByGuidQueryBuilder _destinationProviderArtifactIdByGuidQueryBuilder = new DestinationProviderArtifactIdByGuidQueryBuilder();

		public JobHistoryRepository(ILog logger, ICompletedJobQueryBuilder completedJobQueryBuilder, IWorkspaceManager workspaceManager, IJobHistoryAccess jobHistoryAccess,
			IJobHistorySummaryModelBuilder summaryModelBuilder, ILibraryFactory libraryFactory)
		{
			_logger = logger;
			_completedJobQueryBuilder = completedJobQueryBuilder;
			_workspaceManager = workspaceManager;
			_jobHistoryAccess = jobHistoryAccess;
			_summaryModelBuilder = summaryModelBuilder;
			_libraryFactory = libraryFactory;
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

				var sourceProviderQuery = _sourceProviderArtifactIdByGuidQueryBuilder.Create(Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID);
				var destinationProviderQuery = _destinationProviderArtifactIdByGuidQueryBuilder.Create(Core.Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID);

				var sourceProvider = _libraryFactory.Create<SourceProvider>(request.WorkspaceArtifactId).Query(sourceProviderQuery);
				var destinationProvider = _libraryFactory.Create<DestinationProvider>(request.WorkspaceArtifactId).Query(destinationProviderQuery);

				Query<RDO> ipQuery = _integrationPointByProvidersQueryBuilder.CreateQuery(sourceProvider[0].ArtifactId, destinationProvider[0].ArtifactId);
				IGenericLibrary<IntegrationPoint> ipLibrary = _libraryFactory.Create<IntegrationPoint>(request.WorkspaceArtifactId);

				var ips = ipLibrary.Query(ipQuery);
				Query<RDO> query = _completedJobQueryBuilder.CreateQuery(request.SortColumnName, (request.SortDescending != null) && request.SortDescending.Value,
					ips.Select(x => x.ArtifactId).ToList());

				IGenericLibrary<Data.JobHistory> library = _libraryFactory.Create<Data.JobHistory>(request.WorkspaceArtifactId);
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