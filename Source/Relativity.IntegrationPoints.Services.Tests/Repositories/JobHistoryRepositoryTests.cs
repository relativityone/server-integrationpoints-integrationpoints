using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services.JobHistory;
using Relativity.IntegrationPoints.Services.Repositories.Implementations;

namespace Relativity.IntegrationPoints.Services.Tests.Repositories
{
    [TestFixture, Category("Unit")]
    public class JobHistoryRepositoryTests : TestBase
    {
        private IRelativityIntegrationPointsRepository _relativityIntegrationPointsRepository;
        private ICompletedJobsHistoryRepository _completedJobsHistoryRepository;
        private IJobHistoryAccess _jobHistoryAccess;
        private IJobHistorySummaryModelBuilder _summaryModelBuilder;
        private JobHistoryRepository _jobHistoryRepository;

        private IFederatedInstanceManager _federatedInstanceManager;
        private IWorkspaceManager _workspaceManager;

        public override void SetUp()
        {
            _relativityIntegrationPointsRepository = Substitute.For<IRelativityIntegrationPointsRepository>();
            _completedJobsHistoryRepository = Substitute.For<ICompletedJobsHistoryRepository>();
            IManagerFactory managerFactory = Substitute.For<IManagerFactory>();
            _workspaceManager = Substitute.For<IWorkspaceManager>();

            _jobHistoryAccess = Substitute.For<IJobHistoryAccess>();
            _summaryModelBuilder = Substitute.For<IJobHistorySummaryModelBuilder>();
            IDestinationParser destinationParser = new DestinationParser();
            _federatedInstanceManager = Substitute.For<IFederatedInstanceManager>();

            _jobHistoryRepository = new JobHistoryRepository(
                _relativityIntegrationPointsRepository,
                _completedJobsHistoryRepository,
                managerFactory,
                _jobHistoryAccess,
                _summaryModelBuilder,
                destinationParser);

            managerFactory.CreateFederatedInstanceManager().Returns(_federatedInstanceManager);

            managerFactory.CreateWorkspaceManager().Returns(_workspaceManager);
        }

        [Test]
        public void GoldWorkflow()
        {
            string localInstance = "This Instance";
            string localworkspace1 = "Workspace1 - 1";
            string localworkspace2 = "Workspace2 - 4";
            var integrationPoint1 = new kCura.IntegrationPoints.Core.Models.IntegrationPointModel { ArtifactID = 1 };
            var integrationPoint2 = new kCura.IntegrationPoints.Core.Models.IntegrationPointModel { ArtifactID = 2 };

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

            var integrationPoints = new List<kCura.IntegrationPoints.Core.Models.IntegrationPointModel>
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
                new JobHistoryModel {DestinationWorkspace = localworkspace2, DestinationInstance = localInstance},
                new JobHistoryModel {DestinationWorkspace = localworkspace1, DestinationInstance = localInstance}
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

            _relativityIntegrationPointsRepository.RetrieveIntegrationPoints().Returns(integrationPoints);
            _completedJobsHistoryRepository.RetrieveCompleteJobsForIntegrationPoint(request, integrationPoint1.ArtifactID).Returns(queryResult1);
            _completedJobsHistoryRepository.RetrieveCompleteJobsForIntegrationPoint(request, integrationPoint2.ArtifactID).Returns(queryResult2);

            var expectedJobHistories = new List<JobHistoryModel>();
            expectedJobHistories.AddRange(queryResult1);
            expectedJobHistories.AddRange(queryResult2);

            var expectedWorkspaces = new Dictionary<int, IList<int>>
            {
                {-1, new List<int> {1, 2, 3}},
            };

            _jobHistoryAccess.Filter(Arg.Do<List<JobHistoryModel>>(x => CollectionAssert.AreEquivalent(x, expectedJobHistories)),
                Arg.Do<IDictionary<int, IList<int>>>(x => CollectionAssert.AreEquivalent(x, expectedWorkspaces))).Returns(filteredJobHistories);
            _summaryModelBuilder.Create(request.Page, request.PageSize, Arg.Do<IList<JobHistoryModel>>(x => CollectionAssert.AreEquivalent(x, sortedJobHistories))).Returns(expectedResult);

            // act
            JobHistorySummaryModel actualResult = _jobHistoryRepository.GetJobHistory(request);

            // assert
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

            var integrationPoints = new List<kCura.IntegrationPoints.Core.Models.IntegrationPointModel>();
            var queryResult = new List<JobHistoryModel>();

            _relativityIntegrationPointsRepository.RetrieveIntegrationPoints().Returns(integrationPoints);

            _completedJobsHistoryRepository.RetrieveCompleteJobsForIntegrationPoints(request, Arg.Any<List<int>>()).Returns(queryResult);

            _jobHistoryRepository.GetJobHistory(request);

            _workspaceManager.Received(0).GetUserWorkspaces();
        }
    }
}