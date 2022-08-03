using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using NUnit.Framework;
using Relativity.Services;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.FunctionalTests.SystemTests.Repositories
{
    [TestFixture]
    [Feature.DataTransfer.IntegrationPoints]
    public class IntegrationPointRepositoryTests
    {
        private IIntegrationPointRepository _sut;
        private IRelativityObjectManager _objectManager;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _sut = SystemTestsSetupFixture.Container.Resolve<IIntegrationPointRepository>();
            _objectManager = SystemTestsSetupFixture.Container.Resolve<IRelativityObjectManager>();
        }

        [IdentifiedTest("e9f2a23c-3eb8-4abc-ba8d-6fe32cfa7d60")]
        public async Task Delete_ShouldDeleteIntegrationPointWithJobHistory()
        {
            // ARRANGE
            IntegrationPointModelSlim model = CreateIntegrationPointWithJobHistoryAndJobHistoryError();

            // ACT
            _sut.Delete(model.IntegrationPointID);

            // ASSERT
            bool integrationPointExists = await CheckIfIntegrationPointExistsAsync(model.IntegrationPointID).ConfigureAwait(false);
            integrationPointExists.Should().BeFalse("because integration points should be deleted");

            // TEARDOWN
            _objectManager.Delete(model.JobHistoryID);
        }

        [IdentifiedTest("8c13cfb5-42d1-47de-91fb-6c7cc1858432")]
        public async Task Delete_ShouldDeleteIntegrationPointWithoutJobHistory()
        {
            // ARRANGE
            int integrationPointID = CreateDummyIntegrationPoint();

            // ACT
            _sut.Delete(integrationPointID);

            // ASSERT
            bool integrationPointExists = await CheckIfIntegrationPointExistsAsync(integrationPointID).ConfigureAwait(false);
            integrationPointExists.Should().BeFalse("because integration points should be deleted");
        }
        
        [IdentifiedTest("513f1d3b-d1fb-49a4-919e-b0a6ef33529d")]
        public void Delete_ShouldUnlinkJobHistoryButNotDeleteIt()
        {
            // ARRANGE
            IntegrationPointModelSlim model = CreateIntegrationPointWithJobHistoryAndJobHistoryError();

            // ACT
            _sut.Delete(model.IntegrationPointID);

            // ASSERT
            kCura.IntegrationPoints.Data.JobHistory jobHistory = _objectManager.Read<kCura.IntegrationPoints.Data.JobHistory>(model.JobHistoryID);

            jobHistory.Should().NotBeNull();
            jobHistory.IntegrationPoint.Should().BeNullOrEmpty();

            // TEARDOWN
            _objectManager.Delete(model.JobHistoryID);
        }

        [IdentifiedTest("d8447c19-db91-4d65-82d5-19f90e3a6d7b")]
        public async Task Delete_ShouldLeaveJobHistoryErrors()
        {
            // ARRANGE
            IntegrationPointModelSlim model = CreateIntegrationPointWithJobHistoryAndJobHistoryError();

            // ACT
            _sut.Delete(model.IntegrationPointID);

            // ASSERT
            var query = new QueryRequest
            {
                Condition = $"'{ArtifactQueryFieldNames.ArtifactID}' == {model.JobHistoryErrorID}"
            };
            List<JobHistoryError> jobHistoryErrors = await _objectManager
                .QueryAsync<JobHistoryError>(query)
                .ConfigureAwait(false);

            jobHistoryErrors.Should().NotBeEmpty();

            // TEARDOWN
            _objectManager.Delete(model.JobHistoryID);
        }

        private IntegrationPointModelSlim CreateIntegrationPointWithJobHistoryAndJobHistoryError()
        {
            int integrationPointID = CreateDummyIntegrationPoint();
            int jobHistoryID = CreateDummyJobHistory(integrationPointID);
            int jobHistoryErrorID = CreateDummyHistoryError(jobHistoryID);

            return new IntegrationPointModelSlim(integrationPointID, jobHistoryID, jobHistoryErrorID);
        }

        private int CreateDummyIntegrationPoint()
        {
            var integrationPoint = new IntegrationPoint
            {
                Name = "Dummy integration point",
            };

            return _objectManager.Create(integrationPoint);
        }

        private int CreateDummyJobHistory(int integrationPointArtifactID)
        {
            var jobHistory = new kCura.IntegrationPoints.Data.JobHistory
            {
                Name = "Dummy Job History",
                IntegrationPoint = new[] { integrationPointArtifactID }
            };

            return _objectManager.Create(jobHistory);
        }

        private int CreateDummyHistoryError(int jobHistoryArtifactId)
        {
            var jobHistoryError = new JobHistoryError
            {
                ParentArtifactId = jobHistoryArtifactId,
                Name = "Dummy Job History Error",
            };

            return _objectManager.Create(jobHistoryError);
        }

        private async Task<bool> CheckIfIntegrationPointExistsAsync(int artifactID)
        {
            var queryRequest = new QueryRequest
            {
                Condition = $"'ArtifactID' == {artifactID}"
            };
            List<IntegrationPoint> integrationPoints = await _objectManager
                .QueryAsync<IntegrationPoint>(queryRequest, noFields: true)
                .ConfigureAwait(false);
            return integrationPoints.Any();
        }

        private class IntegrationPointModelSlim
        {
            public IntegrationPointModelSlim(int integrationPointId, int jobHistoryId, int jobHistoryErrorId)
            {
                IntegrationPointID = integrationPointId;
                JobHistoryID = jobHistoryId;
                JobHistoryErrorID = jobHistoryErrorId;
            }

            public int IntegrationPointID { get; set; }
            public int JobHistoryID { get; set; }
            public int JobHistoryErrorID { get; set; }
        }
    }
}