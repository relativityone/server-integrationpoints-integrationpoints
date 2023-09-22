using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.UtilityDTO;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.RelativitySync.Tests
{
    [TestFixture, Category("Unit")]
    public class JobHistorySyncServiceTests
    {
        private JobHistorySyncService _sut;
        private Mock<IExtendedJob> _jobFake;
        private Mock<IRelativityObjectManager> _relativityObjectManagerFake;
        private const int _JOB_ID = 1;
        private const int _JOB_HISTORY_ID = 10;
        private const int _WORKSPACE_ID = 100;
        private const int _INTEGRATION_POINT_ID = 110;
        private JobHistory _jobHistory;
        private List<JobHistory> _jobHistoryList;

        [SetUp]
        public void SetUp()
        {
            _jobHistory = new JobHistory { ItemsWithErrors = 0 };
            _jobHistoryList = new List<JobHistory>
            {
                _jobHistory
            };

            _jobFake = new Mock<IExtendedJob>();
            _jobFake.SetupGet(x => x.JobHistoryId).Returns(_JOB_HISTORY_ID);
            _jobFake.SetupGet(x => x.WorkspaceId).Returns(_WORKSPACE_ID);
            _jobFake.SetupGet(x => x.IntegrationPointId).Returns(_INTEGRATION_POINT_ID);
            _jobFake.SetupGet(x => x.JobId).Returns(_JOB_ID);

            _relativityObjectManagerFake = new Mock<IRelativityObjectManager>();
            _relativityObjectManagerFake.Setup(x => x.QueryAsync<JobHistory>(It.IsAny<QueryRequest>(), It.IsAny<bool>(), It.IsAny<ExecutionIdentity>()))
                .ReturnsAsync(_jobHistoryList);
            _relativityObjectManagerFake.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<ExecutionIdentity>()))
                .ReturnsAsync(new ResultSet<RelativityObject>());

            _sut = new JobHistorySyncService(_relativityObjectManagerFake.Object);
        }

        [Test]
        public async Task GetLastCompletedJobHistoryForRunAsync_ShouldReturnRecentJobHistory()
        {
            int workspaceId = 1000;
            int integrationPointId = 111;
            DateTime baseTime = new DateTime(2000, 1, 1);

            // Arrange
            const int numberOfJobHistories = 3;

            List<RelativityObject> response = Enumerable
                .Range(1, numberOfJobHistories)
                .Select(x => new RelativityObject()
                {
                    FieldValues = new List<FieldValuePair>()
                    {
                        new FieldValuePair()
                        {
                            Field = new Field()
                            {
                                Guids = new List<Guid>()
                                {
                                    JobHistoryFieldGuids.StartTimeUTCGuid
                                }
                            },
                            Value = baseTime.AddMinutes(x)
                        }
                    }
                })
                .Reverse()
                .ToList();

            _relativityObjectManagerFake
                .Setup(x => x.QueryAsync(It.Is<QueryRequest>(request =>
                    request.ObjectType.Guid == ObjectTypeGuids.JobHistoryGuid &&
                    request.Sorts.Single().Direction == SortEnum.Descending &&
                    request.Sorts.Single().FieldIdentifier.Guid == JobHistoryFieldGuids.StartTimeUTCGuid &&
                    request.Condition == $"('Integration Point' INTERSECTS MULTIOBJECT [{integrationPointId}]) AND ('End Time (UTC)' ISSET) AND ('Job Status' IN CHOICE [c7d1eb34-166e-48d0-bce7-0be0df43511c]) AND ('Job Type' IN CHOICE [86c8c17d-74ec-4187-bdb1-9380252f4c20])"), It.IsAny<ExecutionIdentity>()))
                .ReturnsAsync(response);

            // Act
            DateTime? actualDate = await _sut.GetLastCompletedJobHistoryForRunDateAsync(workspaceId, integrationPointId)
                .ConfigureAwait(false);

            // Assert
            actualDate.Should().NotBeNull();
            actualDate.Should().Be(baseTime.AddMinutes(numberOfJobHistories));
        }
    }
}
