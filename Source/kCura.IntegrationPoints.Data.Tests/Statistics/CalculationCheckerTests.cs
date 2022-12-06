using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Statistics;
using kCura.IntegrationPoints.Data.Statistics.Implementations;
using kCura.IntegrationPoints.Data.UtilityDTO;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using static kCura.Utility.DirectoryElements;

namespace kCura.IntegrationPoints.Data.Tests.Statistics
{
    [TestFixture]
    [Category("Unit")]
    public class CalculationCheckerTests
    {
        private readonly int _INTEGRATION_POINT_ID = -10;

        private Mock<IAPILog> _loggerMock;
        private Mock<IRelativityObjectManager> _objectManagerMock;

        private CalculationChecker _sut;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<IAPILog>();
            _objectManagerMock = new Mock<IRelativityObjectManager>();

            _sut = new CalculationChecker(_objectManagerMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task GetCalculationState_ShouldCreateNewState_WhenIntegrationPointFieldValueIsNull()
        {
            // Arrange
            GetDataSetup(null);

            // Act
            CalculationState result = await _sut.GetCalculationState(_INTEGRATION_POINT_ID).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            _objectManagerMock.Verify(
                x => x.QueryAsync(
                It.IsAny<QueryRequest>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<ExecutionIdentity>()),
                Times.Once);
            result.Status.Should().Be(CalculationStatus.New);
        }

        [Test]
        public async Task GetCalculationState_ShouldReturnError_WhenObjectManagerCallFails()
        {
            // Arrange
            CalculationStatus expectedStatus = CalculationStatus.Error;
            GetDataSetup(new CalculationState { Status = CalculationStatus.InProgress });

            _objectManagerMock.Setup(x => x.QueryAsync(
                It.IsAny<QueryRequest>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<ExecutionIdentity>()))
                .Throws(new Exception());

            // Act
            CalculationState result = await _sut.GetCalculationState(_INTEGRATION_POINT_ID).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            _objectManagerMock.Verify(
                x => x.QueryAsync(
                It.IsAny<QueryRequest>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
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
            CalculationState result = await _sut.MarkAsCalculating(_INTEGRATION_POINT_ID).ConfigureAwait(false);

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
            CalculationState result = await _sut.MarkAsCalculating(_INTEGRATION_POINT_ID).ConfigureAwait(false);

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
            string expectedCalculationDate = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture);
            GetDataSetup(new CalculationState { Status = CalculationStatus.InProgress });
            UpdateDataSetup(true);

            // Act
            CalculationState result = await _sut.MarkCalculationAsFinished(_INTEGRATION_POINT_ID, new DocumentsStatistics()).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            _objectManagerMock.Verify(
                x => x.QueryAsync(
                It.IsAny<QueryRequest>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
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
            CalculationState result = await _sut.MarkCalculationAsFinished(_INTEGRATION_POINT_ID, new DocumentsStatistics()).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            _objectManagerMock.Verify(
                x => x.QueryAsync(
                It.IsAny<QueryRequest>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
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
            CalculationState result = await _sut.MarkCalculationAsFinished(_INTEGRATION_POINT_ID, new DocumentsStatistics()).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            _objectManagerMock.Verify(
                x => x.QueryAsync(
                It.IsAny<QueryRequest>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
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
        public async Task MarkCalculationAsCancelled_ShouldCorrectlySetStatusToCancelled()
        {
            // Arrange
            CalculationStatus expectedStatus = CalculationStatus.Canceled;
            UpdateDataSetup();

            // Act
            CalculationState result = await _sut.MarkCalculationAsCancelled(_INTEGRATION_POINT_ID).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            _objectManagerMock.Verify(
                x => x.UpdateAsync(
                It.IsAny<int>(),
                It.IsAny<IList<FieldRefValuePair>>(),
                It.IsAny<ExecutionIdentity>()), Times.Once);
            result.Status.Should().Be(expectedStatus);
        }

        private void GetDataSetup(CalculationState state)
        {
            string input = state == null ? null : JsonConvert.SerializeObject(state);
            IntegrationPoint integrationPoint = new Data.IntegrationPoint
            {
                CalculationState = JsonConvert.SerializeObject(state)
            };

            RelativityObject relativityObject = new RelativityObject
            {
                FieldValues = new List<FieldValuePair>() { new FieldValuePair { Value = input } }
            };

            ResultSet<RelativityObject> resultSet = new ResultSet<RelativityObject>();
            resultSet.Items = new List<RelativityObject>() { relativityObject };

            _objectManagerMock.Setup(x => x.QueryAsync(
                It.IsAny<QueryRequest>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<ExecutionIdentity>()))
                .ReturnsAsync(resultSet);
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
