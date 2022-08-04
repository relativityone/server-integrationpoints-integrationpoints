using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.TestCategories;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Helpers;
using kCura.IntegrationPoints.Data.Repositories;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Identification;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;

namespace Relativity.IntegrationPoints.FunctionalTests.SystemTests.JobHistory
{
    [TestFixture]
    [Feature.DataTransfer.IntegrationPoints]
    public class JobHistoryManagerTests
    {
        private JobHistoryManager _sut;

        private int _workspaceID => SystemTestsSetupFixture.SourceWorkspace.ArtifactID;
        private IRelativityObjectManager _objectManager;

        private List<int> _artifactsIDsToDelete;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            IWindsorContainer container = SystemTestsSetupFixture.Container;

            IRelativityObjectManagerFactory objectManagerFactory = container.Resolve<IRelativityObjectManagerFactory>();
            _objectManager = objectManagerFactory.CreateRelativityObjectManager(_workspaceID);

            IMassUpdateHelper massUpdateHelper = container.Resolve<IMassUpdateHelper>();
            IAPILog logger = container.Resolve<IAPILog>();
            IRepositoryFactory repositoryFactory = container.Resolve<IRepositoryFactory>();

            _sut = new JobHistoryManager(
                repositoryFactory,
                logger,
                massUpdateHelper);
        }

        [SetUp]
        public void SetUp()
        {
            _artifactsIDsToDelete = new List<int>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (int artifactID in _artifactsIDsToDelete)
            {
                _objectManager.Delete(artifactID);
            }
        }

        [IdentifiedTestCase("e5b734d9-5020-47f5-ab78-34363fb186a3", 0, 0)]
        [IdentifiedTestCase("5e306922-4fbc-4172-adf8-c6470b165485", 0, 1)]
        [IdentifiedTestCase("8ee8815c-ff17-4917-becd-e083bc3a592a", 1, 0)]
        [IdentifiedTestCase("a9bda829-054a-440f-9f37-22d89893566f", 1, 1)]
        [IdentifiedTestCase("f8f1b639-4f50-488a-984f-c75efb899eb2", 7, 15)]
        [IdentifiedTestCase("0217d61c-203c-4eae-b293-4aa40ce1f512", 1, 75000, Category = TestCategories.STRESS_TEST, Explicit = true)]
        public async Task ShouldSetErrorStatusesToExpired(
            int numberOfJobLevelErrors,
            int numberOfItemLevelErrors)
        {
            // arrange
            int jobHistoryArtifactID = CreateDummyJobHistory();

            CreateJobHistoryErrorsForJobHistory(
                jobHistoryArtifactID,
                ErrorTypeChoices.JobHistoryErrorJob,
                numberOfJobLevelErrors);
            CreateJobHistoryErrorsForJobHistory(
                jobHistoryArtifactID,
                ErrorTypeChoices.JobHistoryErrorItem,
                numberOfItemLevelErrors);

            // act
            await _sut
                .SetErrorStatusesToExpiredAsync(_workspaceID, jobHistoryArtifactID)
                .ConfigureAwait(false);

            // assert
            List<JobHistoryError> allJobHistoryErrorsForJobHistory = await
                GetAllJobHistoryErrorsForJobHistoryAsync(jobHistoryArtifactID)
                    .ConfigureAwait(false);

            int expectedNumberOfJobHistoryErrors = numberOfJobLevelErrors + numberOfItemLevelErrors;
            allJobHistoryErrorsForJobHistory.Should().HaveCount(expectedNumberOfJobHistoryErrors);

            allJobHistoryErrorsForJobHistory
                .Select(x => x.ErrorStatus.Guids.Single())
                .ShouldAllBeEquivalentTo(ErrorStatusChoices.JobHistoryErrorExpiredGuid);
        }

        private void CreateJobHistoryErrorsForJobHistory(
            int jobHistoryArtifactID,
            ChoiceRef errorType,
            int numberOfErrorsToCreate)
        {
            for (int i = 0; i < numberOfErrorsToCreate; i++)
            {
                CreateJobHistoryErrorForJobHistory(
                    jobHistoryArtifactID,
                    errorType,
                    ID: i);
            }
        }

        private void CreateJobHistoryErrorForJobHistory(
            int jobHistoryArtifactID,
            ChoiceRef errorType,
            int ID)
        {
            var jobHistoryError = new JobHistoryError
            {
                Name = $"{errorType.Name} level error - {ID}",
                JobHistory = jobHistoryArtifactID,
                ParentArtifactId = jobHistoryArtifactID,
                ErrorStatus = ErrorStatusChoices.JobHistoryErrorNew,
                ErrorType = ErrorTypeChoices.JobHistoryErrorItem
            };
            _objectManager.Create(jobHistoryError);
        }

        private int CreateDummyJobHistory()
        {
            int integrationPointArtifactID = CreateDummyIntegrationPoints();
            return CreateDummyJobHistoryForIntegrationPoint(integrationPointArtifactID);
        }

        private int CreateDummyJobHistoryForIntegrationPoint(int integrationPointArtifactID)
        {
            var jobHistory = new kCura.IntegrationPoints.Data.JobHistory
            {
                IntegrationPoint = new[] { integrationPointArtifactID },
                Name = $"Dummy - {Guid.NewGuid()}"
            };

            int artifactID = _objectManager.Create(jobHistory);
            _artifactsIDsToDelete.Add(artifactID);
            return artifactID;
        }

        private int CreateDummyIntegrationPoints()
        {
            var integrationPoint = new IntegrationPoint
            {
                Name = $"Dummy - {Guid.NewGuid()}"
            };

            int artifactID = _objectManager.Create(integrationPoint);
            _artifactsIDsToDelete.Add(artifactID);
            return artifactID;
        }

        private Task<List<JobHistoryError>> GetAllJobHistoryErrorsForJobHistoryAsync(int jobHistoryArtifactID)
        {
            var queryRequest = new QueryRequest
            {
                Condition = $"'{JobHistoryErrorFields.JobHistory}' == {jobHistoryArtifactID}"
            };
            return _objectManager.QueryAsync<JobHistoryError>(queryRequest);
        }
    }
}
