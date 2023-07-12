using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.IntegrationPointRdoService;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Data;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    [Category("Unit")]
    public class JobHistoryServiceTests
    {
        private Mock<IKeplerServiceFactory> _keplerServiceFactory;
        private Mock<IObjectManager> _objectManager;
        private Mock<IIntegrationPointRdoService> _integrationPointRdoService;
        private Mock<IDateTime> _dateTime;
        private Mock<IAPILog> _logger;
        private JobHistoryService _sut;

        [SetUp]
        public void SetUp()
        {
            _objectManager = new Mock<IObjectManager>();
            _objectManager
                .Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateRequest>()))
                .ReturnsAsync(new UpdateResult()
                {
                    EventHandlerStatuses = new List<EventHandlerStatus>()
                });

            _integrationPointRdoService = new Mock<IIntegrationPointRdoService>();

            _dateTime = new Mock<IDateTime>();

            _keplerServiceFactory = new Mock<IKeplerServiceFactory>();
            _keplerServiceFactory
                .Setup(x => x.CreateProxyAsync<IObjectManager>())
                .ReturnsAsync(_objectManager.Object);

            _logger = new Mock<IAPILog>();
            _logger
                .Setup(x => x.ForContext<JobHistoryService>())
                .Returns(_logger.Object);

            _sut = new JobHistoryService(_keplerServiceFactory.Object, _dateTime.Object, _integrationPointRdoService.Object, _logger.Object);
        }

        [Test]
        public async Task UpdateStatusAsync_ShouldSendValidRequest()
        {
            // Arrange
            int workspaceId = 111;
            int jobHistoryId = 222;
            int integrationPointId = 333;
            Guid expectedStatusGuid = JobStatusChoices.JobHistoryProcessingGuid;

            // Act
            await _sut.UpdateStatusAsync(workspaceId, integrationPointId, jobHistoryId, expectedStatusGuid);

            // Assert
            _objectManager
                .Verify(x => x.UpdateAsync(workspaceId, It.Is<UpdateRequest>(request =>
                    request.Object.ArtifactID == jobHistoryId &&
                    request.FieldValues.Single().Field.Guid == JobHistoryFieldGuids.JobStatusGuid &&
                    ((ChoiceRef)request.FieldValues.Single().Value).Guid == expectedStatusGuid)));
        }

        [TestCaseSource(nameof(StatusesThatShouldNotUpdateHasErrors))]
        public async Task UpdateStatusAsync_ShouldNotUpdateHasErrors(Guid statusGuid)
        {
            // Arrange
            int workspaceId = 111;
            int jobHistoryId = 222;
            int integrationPointId = 333;

            // Act
            await _sut.UpdateStatusAsync(workspaceId, integrationPointId, jobHistoryId, statusGuid);

            // Assert
            _integrationPointRdoService.Verify(x => x.TryUpdateHasErrorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
        }

        [TestCaseSource(nameof(StatusesThatShouldUpdateHasErrorsToTrue))]
        public async Task UpdateStatusAsync_ShouldUpdateHasErrorsToTrue(Guid statusGuid)
        {
            // Arrange
            int workspaceId = 111;
            int jobHistoryId = 222;
            int integrationPointId = 333;

            // Act
            await _sut.UpdateStatusAsync(workspaceId, integrationPointId, jobHistoryId, statusGuid);

            // Assert
            _integrationPointRdoService.Verify(x => x.TryUpdateHasErrorsAsync(It.IsAny<int>(), It.IsAny<int>(), true), Times.Once);
        }

        [TestCaseSource(nameof(StatusesThatShouldUpdateHasErrorsToFalse))]
        public async Task UpdateStatusAsync_ShouldUpdateHasErrorsToFalse(Guid statusGuid)
        {
            // Arrange
            int workspaceId = 111;
            int jobHistoryId = 222;
            int integrationPointId = 333;

            // Act
            await _sut.UpdateStatusAsync(workspaceId, integrationPointId, jobHistoryId, statusGuid);

            // Assert
            _integrationPointRdoService.Verify(x => x.TryUpdateHasErrorsAsync(It.IsAny<int>(), It.IsAny<int>(), false), Times.Once);
        }

        [Test]
        public async Task TryUpdateStartTimeAsync_ShouldUpdate()
        {
            // Arrange
            int workspaceId = 111;
            int jobHistoryId = 222;

            DateTime startTime = DateTime.UtcNow;
            _dateTime.Setup(x => x.UtcNow).Returns(startTime);

            // Act
            await _sut.TryUpdateStartTimeAsync(workspaceId, jobHistoryId);

            // Assert
            _objectManager
                .Verify(x => x.UpdateAsync(workspaceId, It.Is<UpdateRequest>(request =>
                    request.Object.ArtifactID == jobHistoryId &&
                    request.FieldValues.Single().Field.Guid == JobHistoryFieldGuids.StartTimeUTCGuid &&
                    (DateTime)request.FieldValues.Single().Value == startTime)));
        }

        [Test]
        public void TryUpdateStartTimeAsync_ShouldNotThrow()
        {
            // Arrange
            int workspaceId = 111;
            int jobHistoryId = 222;

            DateTime startTime = DateTime.UtcNow;
            _dateTime.Setup(x => x.UtcNow).Returns(startTime);

            _objectManager
                .Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateRequest>()))
                .Throws<ServiceException>();

            // Act
            Func<Task> action = () => _sut.TryUpdateStartTimeAsync(workspaceId, jobHistoryId);

            // Assert
            action.ShouldNotThrow();
        }

        [Test]
        public async Task TryUpdateEndTimeAsync_ShouldUpdate()
        {
            // Arrange
            int workspaceId = 111;
            int jobHistoryId = 222;
            int integrationPointId = 333;

            DateTime endTime = DateTime.UtcNow;
            _dateTime.Setup(x => x.UtcNow).Returns(endTime);

            // Act
            await _sut.TryUpdateEndTimeAsync(workspaceId, integrationPointId, jobHistoryId);

            // Assert
            _objectManager
                .Verify(x => x.UpdateAsync(workspaceId, It.Is<UpdateRequest>(request =>
                    request.Object.ArtifactID == jobHistoryId &&
                    request.FieldValues.Single().Field.Guid == JobHistoryFieldGuids.EndTimeUTCGuid &&
                    (DateTime)request.FieldValues.Single().Value == endTime)));

            _integrationPointRdoService.Verify(x => x.TryUpdateLastRuntimeAsync(workspaceId, integrationPointId, endTime), Times.Once);
        }

        [Test]
        public void TryUpdateEndTimeAsync_ShouldNotThrow()
        {
            // Arrange
            int workspaceId = 111;
            int jobHistoryId = 222;
            int integrationPointId = 333;

            DateTime endTime = DateTime.UtcNow;
            _dateTime.Setup(x => x.UtcNow).Returns(endTime);

            _objectManager
                .Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateRequest>()))
                .Throws<ServiceException>();

            // Act
            Func<Task> action = () => _sut.TryUpdateEndTimeAsync(workspaceId, integrationPointId, jobHistoryId);

            // Assert
            action.ShouldNotThrow();
        }

        [Test]
        public async Task SetTotalItemsAsync_ShouldSendValidRequest()
        {
            // Arrange
            int workspaceId = 111;
            int jobHistoryId = 222;
            int totalItems = 50000;

            // Act
            await _sut.SetTotalItemsAsync(workspaceId, jobHistoryId, totalItems);

            // Assert
            _objectManager
                .Verify(x => x.UpdateAsync(workspaceId, It.Is<UpdateRequest>(request =>
                    request.Object.ArtifactID == jobHistoryId &&
                    request.FieldValues.Single().Field.Guid == JobHistoryFieldGuids.TotalItemsGuid &&
                    (int)request.FieldValues.Single().Value == totalItems)));
        }

        [Test]
        public async Task UpdateReadItemsCountAsync_ShouldSendValidRequest()
        {
            // Arrange
            int workspaceId = 111;
            int jobHistoryId = 222;
            int readItemsCount = 50000;

            // Act
            await _sut.UpdateReadItemsCountAsync(workspaceId, jobHistoryId, readItemsCount);

            // Assert
            _objectManager
                .Verify(x => x.UpdateAsync(workspaceId, It.Is<UpdateRequest>(request =>
                    request.Object.ArtifactID == jobHistoryId &&
                    request.FieldValues.Single().Field.Guid == JobHistoryFieldGuids.ItemsReadGuid &&
                    (int)request.FieldValues.Single().Value == readItemsCount)));
        }

        [Test]
        public async Task UpdateProgressAsync_ShouldSendValidRequest()
        {
            // Arrange
            int workspaceId = 111;
            int jobHistoryId = 222;
            int transferredItemsCount = 250;
            int failedItemsCount = 300;

            // Act
            await _sut.UpdateProgressAsync(workspaceId, jobHistoryId, transferredItemsCount, failedItemsCount);

            // Assert
            _objectManager
                .Verify(x => x.UpdateAsync(workspaceId, It.Is<UpdateRequest>(request =>
                    request.Object.ArtifactID == jobHistoryId &&
                    request.FieldValues.Any(field => field.Field.Guid == JobHistoryFieldGuids.ItemsTransferredGuid && (int)field.Value == transferredItemsCount) &&
                    request.FieldValues.Any(field => field.Field.Guid == JobHistoryFieldGuids.ItemsWithErrorsGuid && (int)field.Value == failedItemsCount))));
        }

        private static IEnumerable<Guid> StatusesThatShouldNotUpdateHasErrors()
        {
            return new[]
            {
                JobStatusChoices.JobHistoryValidatingGuid,
                JobStatusChoices.JobHistoryPendingGuid,
                JobStatusChoices.JobHistoryProcessingGuid,
                JobStatusChoices.JobHistoryStoppingGuid,
                JobStatusChoices.JobHistoryStoppedGuid,
                JobStatusChoices.JobHistorySuspendingGuid,
                JobStatusChoices.JobHistorySuspendedGuid,
            };
        }

        private static IEnumerable<Guid> StatusesThatShouldUpdateHasErrorsToTrue()
        {
            return new[]
            {
                JobStatusChoices.JobHistoryValidationFailedGuid,
                JobStatusChoices.JobHistoryCompletedWithErrorsGuid,
                JobStatusChoices.JobHistoryErrorJobFailedGuid
            };
        }

        private static IEnumerable<Guid> StatusesThatShouldUpdateHasErrorsToFalse()
        {
            return new[]
            {
                JobStatusChoices.JobHistoryCompletedGuid,
            };
        }
    }
}
