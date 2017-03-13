using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Services.JobHistory;
using kCura.IntegrationPoints.Services.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Services.Tests.Repositories
{
	public class JobHistoryRepositoryTests : TestBase
	{
		private IHelper _helper;
		private IHelper _targetHelper;
		private IHelperFactory _helperFactory;
		private IRelativityIntegrationPointsRepository _relativityIntegrationPointsRepository;
		private ICompletedJobsHistoryRepository _completedJobsHistoryRepository;
		private IManagerFactory _managerFactory;
		private IContextContainerFactory _contextContainerFactory;
		private IJobHistoryAccess _jobHistoryAccess;
		private IJobHistorySummaryModelBuilder _summaryModelBuilder;
		private JobHistoryRepository _jobHistoryRepository;
		private IDestinationParser _destinationParser;
		private IFederatedInstanceManager _federatedInstanceManager;
		private IWorkspaceManager _workspaceManager;
		private IWorkspaceManager _targetWorkspaceManager;

		public override void SetUp()
		{
			_helper = Substitute.For<IHelper>();
			_targetHelper = Substitute.For<IHelper>();
			_helperFactory = Substitute.For<IHelperFactory>();
			_relativityIntegrationPointsRepository = Substitute.For<IRelativityIntegrationPointsRepository>();
			_completedJobsHistoryRepository = Substitute.For<ICompletedJobsHistoryRepository>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_workspaceManager = Substitute.For<IWorkspaceManager>();
			_targetWorkspaceManager = Substitute.For<IWorkspaceManager>();

			_jobHistoryAccess = Substitute.For<IJobHistoryAccess>();
			_summaryModelBuilder = Substitute.For<IJobHistorySummaryModelBuilder>();
			_destinationParser = new DestinationParser();
			_federatedInstanceManager = Substitute.For<IFederatedInstanceManager>();

			_helperFactory.CreateTargetHelper(_helper, null, Arg.Any<string>()).Returns(_helper);
			_helperFactory.CreateTargetHelper(_helper, Arg.Any<int>(), Arg.Any<string>()).Returns(_targetHelper);

			_jobHistoryRepository = new JobHistoryRepository(_helper, _helperFactory, _relativityIntegrationPointsRepository, _completedJobsHistoryRepository,
				_managerFactory, _contextContainerFactory, _jobHistoryAccess, _summaryModelBuilder, _destinationParser);

			var contextContainer = Substitute.For<IContextContainer>();
			var targetContextContainer = Substitute.For<IContextContainer>();

			_contextContainerFactory.CreateContextContainer(_helper).Returns(contextContainer);
			_contextContainerFactory.CreateContextContainer(_helper, _helper.GetServicesManager()).Returns(contextContainer);
			_contextContainerFactory.CreateContextContainer(_helper, _targetHelper.GetServicesManager())
				.Returns(targetContextContainer);

			_managerFactory.CreateFederatedInstanceManager(contextContainer).Returns(_federatedInstanceManager);

			_managerFactory.CreateWorkspaceManager(contextContainer).Returns(_workspaceManager);
			_managerFactory.CreateWorkspaceManager(targetContextContainer).Returns(_targetWorkspaceManager);
		}

		[Test]
		public void GoldWorkflow()
		{
			string otherInstanceName = "Other Instance";
			string localInstance = "This Instance";
			string otherInstance = $"{otherInstanceName} - 333";
			string localworkspace1 = "Workspace1 - 1";
			string localworkspace2 = "Workspace2 - 4";
			string otherworkspace1 = "Workspace1 - 11";
			string otherworkspace2 = "Workspace2 - 14";
			var integrationPoint1 = new Core.Models.IntegrationPointModel {ArtifactID = 1};
			var integrationPoint2 = new Core.Models.IntegrationPointModel {ArtifactID = 2};

			var request = new JobHistoryRequest
			{
				WorkspaceArtifactId = 531,
				SortColumnName = "DestinationWorkspace",
				PageSize = 10,
				Page = 0,
				SortDescending = true
			};
			var workspaces = new List<WorkspaceDTO>
			{
				new WorkspaceDTO {ArtifactId = 1},
				new WorkspaceDTO {ArtifactId = 2},
				new WorkspaceDTO {ArtifactId = 3}
			};
			var targetWorkspaces = new List<WorkspaceDTO>
			{
				new WorkspaceDTO {ArtifactId = 11},
				new WorkspaceDTO {ArtifactId = 12},
				new WorkspaceDTO {ArtifactId = 13}
			};
			var integrationPoints = new List<Core.Models.IntegrationPointModel>
			{
				integrationPoint1,
				integrationPoint2
			};
			var queryResult1 = new List<JobHistoryModel>
			{
				new JobHistoryModel {DestinationWorkspace = localworkspace1, DestinationInstance = localInstance},
				new JobHistoryModel {DestinationWorkspace = localworkspace2, DestinationInstance = localInstance}
			};
			var queryResult2 = new List<JobHistoryModel>
			{
				new JobHistoryModel {DestinationWorkspace = otherworkspace1, DestinationInstance = otherInstance},
				new JobHistoryModel {DestinationWorkspace = otherworkspace2, DestinationInstance = otherInstance}
			};
			var filteredJobHistories = new List<JobHistoryModel>()
			{
				queryResult1[0],
				queryResult2[0]
			};
			var sortedJobHistories = new List<JobHistoryModel>()
			{
				queryResult2[0],
				queryResult1[0]
			};

			var expectedResult = new JobHistorySummaryModel();
			
			_workspaceManager.GetUserWorkspaces().Returns(workspaces);
			_targetWorkspaceManager.GetUserWorkspaces().Returns(targetWorkspaces);

			_relativityIntegrationPointsRepository.RetrieveIntegrationPoints(request.WorkspaceArtifactId).Returns(integrationPoints);
			_completedJobsHistoryRepository.RetrieveCompleteJobsForIntegrationPoint(request, integrationPoint1.ArtifactID).Returns(queryResult1);
			_completedJobsHistoryRepository.RetrieveCompleteJobsForIntegrationPoint(request, integrationPoint2.ArtifactID).Returns(queryResult2);

			var expectedJobHistories = new List<JobHistoryModel>();
			expectedJobHistories.AddRange(queryResult1);
			expectedJobHistories.AddRange(queryResult2);

			var expectedWorkspaces = new Dictionary<int, IList<int>>
			{
				{-1, new List<int> {1, 2, 3}},
				{333, new List<int> {11, 12, 13}}
			};

			_jobHistoryAccess.Filter(Arg.Do<List<JobHistoryModel>>(x => CollectionAssert.AreEquivalent(x, expectedJobHistories)),
				Arg.Do<IDictionary<int, IList<int>>>(x => CollectionAssert.AreEquivalent(x, expectedWorkspaces))).Returns(filteredJobHistories);
			_summaryModelBuilder.Create(request.Page, request.PageSize, Arg.Do<IList<JobHistoryModel>>(x => CollectionAssert.AreEquivalent(x, sortedJobHistories))).Returns(expectedResult);

			var actualResult = _jobHistoryRepository.GetJobHistory(request);

			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void SkipWorkflowWhenNoJobsReturned()
		{
			var request = new JobHistoryRequest
			{
				WorkspaceArtifactId = 531,
				SortColumnName = "sort_column_068",
				PageSize = 10,
				Page = 0,
				SortDescending = true
			};

			var integrationPoints = new List<Core.Models.IntegrationPointModel>();
			var queryResult = new List<JobHistoryModel>();

			_relativityIntegrationPointsRepository.RetrieveIntegrationPoints(request.WorkspaceArtifactId).Returns(integrationPoints);

			_completedJobsHistoryRepository.RetrieveCompleteJobsForIntegrationPoints(request, Arg.Any<List<int>>()).Returns(queryResult);

			_jobHistoryRepository.GetJobHistory(request);

			_federatedInstanceManager.Received(0).RetrieveFederatedInstanceByName(Arg.Any<string>());
			_workspaceManager.Received(0).GetUserWorkspaces();
			_targetWorkspaceManager.Received(0).GetUserWorkspaces();
		}
	}
}