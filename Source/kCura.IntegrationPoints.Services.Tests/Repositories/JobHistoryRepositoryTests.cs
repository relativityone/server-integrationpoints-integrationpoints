using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Services.JobHistory;
using kCura.IntegrationPoints.Services.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Services.Tests.Repositories
{
	public class JobHistoryRepositoryTests : TestBase
	{
		private IRelativityIntegrationPointsRepository _relativityIntegrationPointsRepository;
		private ICompletedJobsHistoryRepository _completedJobsHistoryRepository;
		private IWorkspaceManager _workspaceManager;
		private IJobHistoryAccess _jobHistoryAccess;
		private IJobHistorySummaryModelBuilder _summaryModelBuilder;
		private JobHistoryRepository _jobHistoryRepository;
		private IToggleProvider _toggleProvider;

		public override void SetUp()
		{
			_relativityIntegrationPointsRepository = Substitute.For<IRelativityIntegrationPointsRepository>();
			_completedJobsHistoryRepository = Substitute.For<ICompletedJobsHistoryRepository>();
			_workspaceManager = Substitute.For<IWorkspaceManager>();
			_jobHistoryAccess = Substitute.For<IJobHistoryAccess>();
			_summaryModelBuilder = Substitute.For<IJobHistorySummaryModelBuilder>();
			_toggleProvider = Substitute.For<IToggleProvider>();
			_jobHistoryRepository = new JobHistoryRepository(_relativityIntegrationPointsRepository, _completedJobsHistoryRepository, _workspaceManager, _jobHistoryAccess,
				_summaryModelBuilder, _toggleProvider);
		}

		[Test]
		public void GoldWorkflow()
		{
			var request = new JobHistoryRequest
			{
				WorkspaceArtifactId = 531,
				SortColumnName = "sort_column_068",
				PageSize = 10,
				Page = 0,
				SortDescending = true
			};
			var workspaces = new List<int> {1, 2, 3};
			var integrationPoints = new List<int>();
			var queryResult = new List<JobHistoryModel>();
			var filteredJobHistories = new List<JobHistoryModel>();

			var expectedResult = new JobHistorySummaryModel();

			_workspaceManager.GetIdsOfWorkspacesUserHasPermissionToView().Returns(workspaces);
			_relativityIntegrationPointsRepository.RetrieveRelativityIntegrationPointsIds(request.WorkspaceArtifactId).Returns(integrationPoints);
			_completedJobsHistoryRepository.RetrieveCompleteJobsForIntegrationPoints(request, integrationPoints).Returns(queryResult);
			_jobHistoryAccess.Filter(queryResult, workspaces).Returns(filteredJobHistories);
			_summaryModelBuilder.Create(request.Page, request.PageSize, filteredJobHistories).Returns(expectedResult);

			var actualResult = _jobHistoryRepository.GetJobHistory(request);
			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void SkipWorkflowWhenNoWorkspaceWithAccess()
		{
			_workspaceManager.GetIdsOfWorkspacesUserHasPermissionToView().Returns(new List<int>());

			_jobHistoryRepository.GetJobHistory(new JobHistoryRequest());

			_relativityIntegrationPointsRepository.Received(0).RetrieveRelativityIntegrationPointsIds(Arg.Any<int>());
		}
	}
}