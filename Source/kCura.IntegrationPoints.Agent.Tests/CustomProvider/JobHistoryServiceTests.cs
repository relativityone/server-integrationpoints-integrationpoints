using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Data;
using Moq;
using NUnit.Framework;
using Relativity.API;
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

            _keplerServiceFactory = new Mock<IKeplerServiceFactory>();
            _keplerServiceFactory
                .Setup(x => x.CreateProxyAsync<IObjectManager>())
                .ReturnsAsync(_objectManager.Object);

            _logger = new Mock<IAPILog>();
            _logger
                .Setup(x => x.ForContext<JobHistoryService>())
                .Returns(_logger.Object);

            _sut = new JobHistoryService(_keplerServiceFactory.Object, _logger.Object);
        }

        [Test]
        public async Task UpdateStatusAsync_ShouldSendValidRequest()
        {
            // Arrange
            int workspaceId = 111;
            int jobHistoryId = 222;
            Guid expectedStatusGuid = JobStatusChoices.JobHistoryProcessingGuid;

            // Act
            await _sut.UpdateStatusAsync(workspaceId, jobHistoryId, expectedStatusGuid);

            // Assert
            _objectManager
                .Verify(x => x.UpdateAsync(workspaceId, It.Is<UpdateRequest>(request =>
                    request.Object.ArtifactID == jobHistoryId &&
                    request.FieldValues.Single().Field.Guid == JobHistoryFieldGuids.JobStatusGuid &&
                    ((ChoiceRef)request.FieldValues.Single().Value).Guid == expectedStatusGuid)));
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
    }
}