using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.UtilityDTO;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Validation;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.ScheduleQueue.Core.Tests.Validation
{
    [TestFixture, Category("Unit")]
    public class QueueJobValidatorTests
    {
        private Mock<IRelativityObjectManagerFactory> _objectManagerFactoryMock;
        private Mock<IRelativityObjectManager> _objectManagerFake;
        private const int _TEST_WORKSPACE_ID = 100;
        private const int _TEST_INTEGRATION_POINT_ID = 200;
        private const int _TEST_USER_ID = 300;

        [Test]
        public async Task ValidateAsync_ShouldValidateJobAsValid_WhenContextWorkspaceAndIntegrationPointsExist()
        {
            // Arrange
            Job job = new JobBuilder()
                .WithWorkspaceId(_TEST_WORKSPACE_ID)
                .WithRelatedObjectArtifactId(_TEST_INTEGRATION_POINT_ID)
                .WithSubmittedBy(_TEST_USER_ID)
                .Build();

            QueueJobValidator sut = GetSut();

            SetUpWorkspaceExists(true);
            SetUpIntegrationPointExists(true);
            SetUpUserExists(true);

            // Act
            PreValidationResult result = await sut.ValidateAsync(job).ConfigureAwait(false);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Exception.Should().BeNull();
        }

        [Test]
        public async Task ValidateAsync_ShouldValidateJobAsInvalid_WhenContextWorkspaceDoesNotExist()
        {
            // Arrange
            Job job = new JobBuilder()
                .WithWorkspaceId(_TEST_WORKSPACE_ID)
                .WithRelatedObjectArtifactId(_TEST_INTEGRATION_POINT_ID)
                .WithSubmittedBy(_TEST_USER_ID)
                .Build();

            QueueJobValidator sut = GetSut();

            SetUpWorkspaceExists(false);
            SetUpIntegrationPointExists(false);
            SetUpUserExists(true);

            // Act
            PreValidationResult result = await sut.ValidateAsync(job).ConfigureAwait(false);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Exception.Message.Should().Contain(_TEST_WORKSPACE_ID.ToString());
        }

        [Test]
        public async Task ValidateAsync_ShouldValidateJobAsInvalid_WhenContextIntegrationPointDoesNotExist()
        {
            // Arrange
            Job job = new JobBuilder()
                .WithWorkspaceId(_TEST_WORKSPACE_ID)
                .WithRelatedObjectArtifactId(_TEST_INTEGRATION_POINT_ID)
                .WithSubmittedBy(_TEST_USER_ID)
                .Build();

            QueueJobValidator sut = GetSut();

            SetUpWorkspaceExists(true);
            SetUpIntegrationPointExists(false);
            SetUpUserExists(true);

            // Act
            PreValidationResult result = await sut.ValidateAsync(job).ConfigureAwait(false);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Exception.Message.Should().Contain(_TEST_INTEGRATION_POINT_ID.ToString());
        }

        [Test]
        public async Task ValidateAsync_ShouldNotValidateIntegrationPoint_WhenContextWorkspaceDoesNotExist()
        {
            // Arrange
            Job job = new JobBuilder()
                .WithWorkspaceId(_TEST_WORKSPACE_ID)
                .WithRelatedObjectArtifactId(_TEST_INTEGRATION_POINT_ID)
                .WithSubmittedBy(_TEST_USER_ID)
                .Build();

            QueueJobValidator sut = GetSut();

            SetUpWorkspaceExists(false);
            SetUpIntegrationPointExists(false);
            SetUpUserExists(true);

            // Act
            await sut.ValidateAsync(job).ConfigureAwait(false);

            // Assert
            _objectManagerFake.Verify(
                x => x.QueryAsync(It.IsAny<QueryRequest>(), 0, 1, false, It.IsAny<ExecutionIdentity>()),
                Times.Never());
        }

        private QueueJobValidator GetSut()
        {
            _objectManagerFake = new Mock<IRelativityObjectManager>();
            _objectManagerFactoryMock = new Mock<IRelativityObjectManagerFactory>();
            _objectManagerFactoryMock
                .Setup(x => x.CreateRelativityObjectManager(It.IsAny<int>()))
                .Returns(_objectManagerFake.Object);

            Mock<IConfig> config = new Mock<IConfig>();

            Mock<IScheduleRuleFactory> scheduleRuleFactory = new Mock<IScheduleRuleFactory>();

            Mock<IAPILog> log = new Mock<IAPILog>();
            log.Setup(x => x.ForContext<QueueJobValidator>())
                .Returns(log.Object);

            return new QueueJobValidator(_objectManagerFactoryMock.Object, config.Object, scheduleRuleFactory.Object, log.Object);
        }

        private void SetUpWorkspaceExists(bool exists)
        {
            int expectedTotalCount = exists ? 1 : 0;

            _objectManagerFake.Setup(x => x.QuerySlimAsync(
                    It.Is<QueryRequest>(r =>
                        r.Condition.Contains(_TEST_WORKSPACE_ID.ToString())),
                    0,
                    1,
                    false,
                    It.IsAny<ExecutionIdentity>()))
                .ReturnsAsync(new ResultSet<RelativityObjectSlim>
                {
                    TotalCount = expectedTotalCount
                })
                .Verifiable();
        }

        private void SetUpIntegrationPointExists(bool exists)
        {
            List<RelativityObject> expectedObjects = exists
                ? new List<RelativityObject>() { new RelativityObject() }
                : new List<RelativityObject>();

            _objectManagerFake.Setup(
                    x => x.QueryAsync(
                        It.Is<QueryRequest>(r =>
                            r.ObjectType.Guid == ObjectTypeGuids.IntegrationPointGuid &&
                            r.Condition.Contains(_TEST_INTEGRATION_POINT_ID.ToString())),
                        0,
                        1,
                        false,
                        It.IsAny<ExecutionIdentity>()))
                .ReturnsAsync(new ResultSet<RelativityObject>
                {
                    Items = expectedObjects,
                    TotalCount = expectedObjects.Count
                })
                .Verifiable();
        }

        private void SetUpUserExists(bool exists)
        {
            int expectedTotalCount = exists ? 1 : 0;

            _objectManagerFake.Setup(
                    x => x.QuerySlimAsync(
                    It.Is<QueryRequest>(r => r.Condition.Contains(_TEST_USER_ID.ToString())),
                    0,
                    1,
                    false,
                    It.IsAny<ExecutionIdentity>()))
                .ReturnsAsync(new ResultSet<RelativityObjectSlim>
                {
                    TotalCount = expectedTotalCount
                })
                .Verifiable();
        }
    }
}
