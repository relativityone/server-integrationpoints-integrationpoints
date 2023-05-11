using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Statistics;
using kCura.IntegrationPoints.Data.Statistics.Implementations;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Statistics
{
    [TestFixture]
    [Category("Unit")]
    public class OnDemandStatisticsServiceTests
    {
        private readonly int _INTEGRATION_POINT_ID = -10;
        private readonly int _WORKSPACE_ID = 100;
        private Mock<IAPILog> _loggerMock;
        private Mock<IDateTime> _dateTimeMock;
        private Mock<IRelativityObjectManager> _objectManagerMock;
        private OnDemandStatisticsService _sut;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<IAPILog>();
            _dateTimeMock = new Mock<IDateTime>();
            _objectManagerMock = new Mock<IRelativityObjectManager>();

            Mock<IRelativityObjectManagerFactory> objectManagerFactory = new Mock<IRelativityObjectManagerFactory>();
            objectManagerFactory.Setup(x => x.CreateRelativityObjectManager(_WORKSPACE_ID)).Returns(_objectManagerMock.Object);

            _sut = new OnDemandStatisticsService(objectManagerFactory.Object, _dateTimeMock.Object, _loggerMock.Object);
        }

        [Test]
        public void GetCalculationState_ShouldCreateNewState_WhenIntegrationPointFieldValueIsNull()
        {
            // Arrange
            GetDataSetup(null);

            // Act
            CalculationState result = _sut.GetCalculationState(_WORKSPACE_ID, _INTEGRATION_POINT_ID);

            // Assert
            result.Should().NotBeNull();
            _objectManagerMock.Verify(
                x => x.Read<IntegrationPoint>(
                    _INTEGRATION_POINT_ID,
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<ExecutionIdentity>()),
                Times.Once);
            result.Status.Should().Be(CalculationStatus.New);
        }

        [Test]
        public void GetCalculationState_ShouldReturnError_WhenObjectManagerCallFails()
        {
            // Arrange
            CalculationStatus expectedStatus = CalculationStatus.Error;
            GetDataSetup(new CalculationState { Status = CalculationStatus.InProgress });

            _objectManagerMock.Setup(
                 x => x.Read<IntegrationPoint>(
                     _INTEGRATION_POINT_ID,
                     It.IsAny<IEnumerable<Guid>>(),
                     It.IsAny<ExecutionIdentity>()))
                 .Throws(new Exception());

            // Act
            CalculationState result = _sut.GetCalculationState(_WORKSPACE_ID, _INTEGRATION_POINT_ID);

            // Assert
            result.Should().NotBeNull();
            _objectManagerMock.Verify(
                x => x.Read<IntegrationPoint>(
                    _INTEGRATION_POINT_ID,
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<ExecutionIdentity>()),
                Times.Once);
            result.Status.Should().Be(expectedStatus);
        }

        [Test]
        public async Task MarkAsCalculating_ShouldSetStatusToInProgress_WhenIntegrationPointWasUpdatedSuccessfully()
        {
            // Arrange
            CalculationStatus expectedStatus = CalculationStatus.InProgress;
            UpdateDataSetup();

            // Act
            CalculationState result = await _sut.MarkAsCalculating(_WORKSPACE_ID, _INTEGRATION_POINT_ID).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            _objectManagerMock.Verify(
                x => x.UpdateAsync(
                It.IsAny<int>(),
                It.IsAny<IList<FieldRefValuePair>>(),
                It.IsAny<ExecutionIdentity>()), Times.Once);
            result.Status.Should().Be(expectedStatus);
        }

        [Test]
        public async Task MarkAsCalculating_ShouldSetStatusToError_WhenIntegrationPointUpdateFailed()
        {
            // Arrange
            CalculationStatus expectedStatus = CalculationStatus.Error;
            UpdateDataSetup(false);

            // Act
            CalculationState result = await _sut.MarkAsCalculating(_WORKSPACE_ID, _INTEGRATION_POINT_ID).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            _objectManagerMock.Verify(
                x => x.UpdateAsync(
                It.IsAny<int>(),
                It.IsAny<IList<FieldRefValuePair>>(),
                It.IsAny<ExecutionIdentity>()), Times.Once);
            result.Status.Should().Be(expectedStatus);
        }

        [Test]
        public async Task MarkCalculationAsFinished_ShouldReturnCorrectState_WhenInputIsCorrect()
        {
            // Arrange
            CalculationStatus expectedStatus = CalculationStatus.Completed;
            _dateTimeMock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);
            string expectedCalculationDate = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture);
            GetDataSetup(new CalculationState { Status = CalculationStatus.InProgress });
            UpdateDataSetup(true);

            // Act
            CalculationState result = await _sut.MarkCalculationAsFinished(_WORKSPACE_ID, _INTEGRATION_POINT_ID, new DocumentsStatistics()).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            _objectManagerMock.Verify(
                x => x.Read<IntegrationPoint>(
                    _INTEGRATION_POINT_ID,
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<ExecutionIdentity>()),
                Times.Once);
            _objectManagerMock.Verify(
                x => x.UpdateAsync(
                It.IsAny<int>(),
                It.IsAny<IList<FieldRefValuePair>>(),
                It.IsAny<ExecutionIdentity>()), Times.Once);
            result.Status.Should().Be(expectedStatus);
            result.DocumentStatistics.CalculatedOn.Should().Be(expectedCalculationDate);
        }

        [Test]
        public async Task MarkCalculationAsFinished_ShouldReturnError_WhenCalculationStateIsNotInProgress()
        {
            // Arrange
            CalculationStatus expectedStatus = CalculationStatus.Error;
            GetDataSetup(new CalculationState { Status = CalculationStatus.Canceled });

            // Act
            CalculationState result = await _sut.MarkCalculationAsFinished(_WORKSPACE_ID, _INTEGRATION_POINT_ID, new DocumentsStatistics()).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            _objectManagerMock.Verify(
                x => x.Read<IntegrationPoint>(
                    _INTEGRATION_POINT_ID,
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<ExecutionIdentity>()),
                Times.Once);
            _objectManagerMock.Verify(
                x => x.UpdateAsync(
                It.IsAny<int>(),
                It.IsAny<IList<FieldRefValuePair>>(),
                It.IsAny<ExecutionIdentity>()), Times.Never);
            result.Status.Should().Be(expectedStatus);
        }

        [Test]
        public async Task MarkCalculationAsFinished_ShouldReturnError_WhenCalculationStateHasErrorStatus()
        {
            // Arrange
            CalculationStatus expectedStatus = CalculationStatus.Error;
            GetDataSetup(new CalculationState { Status = CalculationStatus.Error });

            // Act
            CalculationState result = await _sut.MarkCalculationAsFinished(_WORKSPACE_ID, _INTEGRATION_POINT_ID, new DocumentsStatistics()).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            _objectManagerMock.Verify(
                x => x.Read<IntegrationPoint>(
                    _INTEGRATION_POINT_ID,
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<ExecutionIdentity>()),
                Times.Once);
            _objectManagerMock.Verify(
                x => x.UpdateAsync(
                It.IsAny<int>(),
                It.IsAny<IList<FieldRefValuePair>>(),
                It.IsAny<ExecutionIdentity>()), Times.Never);
            result.Status.Should().Be(expectedStatus);
        }

        private void GetDataSetup(CalculationState state)
        {
            string input = state == null ? null : JsonConvert.SerializeObject(state);
            IntegrationPoint integrationPoint = new IntegrationPoint
            {
                CalculationState = input
            };

            IEnumerable<Guid> integrationPointFields = new[]
            {
                IntegrationPointFieldGuids.CalculationStateGuid
            };

            _objectManagerMock.Setup(x => x.Read<IntegrationPoint>(_INTEGRATION_POINT_ID, integrationPointFields, It.IsAny<ExecutionIdentity>())).Returns(integrationPoint);
        }

        private void UpdateDataSetup(bool updatedSuccessfully = true)
        {
            _objectManagerMock.Setup(x => x.UpdateAsync(
                It.IsAny<int>(),
                It.IsAny<IList<FieldRefValuePair>>(),
                It.IsAny<ExecutionIdentity>()))
                .ReturnsAsync(updatedSuccessfully);
        }
    }
}
