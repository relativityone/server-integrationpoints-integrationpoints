using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.JobHistory;
using kCura.IntegrationPoints.Services.Repositories;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services.Tests.Repositories
{
	public class JobHistoryRepositoryTests : TestBase
	{
		private JobHistoryRepository _jobHistoryRepository;
		private ICompletedJobQueryBuilder _completedJobQueryBuilder;
		private IWorkspaceManager _workspaceManager;
		private IJobHistoryAccess _jobHistoryAccess;
		private IJobHistorySummaryModelBuilder _jobHistorySummaryModelBuilder;
		private ILibraryFactory _libraryFactory;
		private IGenericLibrary<Data.JobHistory> _genericLibrary;

		public override void SetUp()
		{
			var logger = Substitute.For<ILog>();

			_completedJobQueryBuilder = Substitute.For<ICompletedJobQueryBuilder>();
			_workspaceManager = Substitute.For<IWorkspaceManager>();
			_jobHistoryAccess = Substitute.For<IJobHistoryAccess>();
			_jobHistorySummaryModelBuilder = Substitute.For<IJobHistorySummaryModelBuilder>();
			_libraryFactory = Substitute.For<ILibraryFactory>();
			_genericLibrary = Substitute.For<IGenericLibrary<Data.JobHistory>>();

			_jobHistoryRepository = new JobHistoryRepository(logger, _completedJobQueryBuilder, _workspaceManager, _jobHistoryAccess, _jobHistorySummaryModelBuilder,
				_libraryFactory);
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
			var query = new Query<RDO>();
			var workspaces = new List<int> {1, 2, 3};
			var queryResult = new List<Data.JobHistory>();
			var filteredJobHistories = new List<Data.JobHistory>();

			var expectedResult = new JobHistorySummaryModel();

			_workspaceManager.GetIdsOfWorkspacesUserHasPermissionToView().Returns(workspaces);
			_completedJobQueryBuilder.CreateQuery(request.SortColumnName, request.SortDescending.Value, new List<int>()).Returns(query);
			_libraryFactory.Create<Data.JobHistory>(request.WorkspaceArtifactId).Returns(_genericLibrary);
			_genericLibrary.Query(query).Returns(queryResult);
			_jobHistoryAccess.Filter(queryResult, workspaces).Returns(filteredJobHistories);
			_jobHistorySummaryModelBuilder.Create(request.Page, request.PageSize, filteredJobHistories).Returns(expectedResult);

			var actualResult = _jobHistoryRepository.GetJobHistory(request);
			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void SkipWorkflowWhenNoWorkspaceWithAccess()
		{
			_workspaceManager.GetIdsOfWorkspacesUserHasPermissionToView().Returns(new List<int>());

			_jobHistoryRepository.GetJobHistory(new JobHistoryRequest());

			_completedJobQueryBuilder.Received(0).CreateQuery(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<List<int>>());
		}
	}
}