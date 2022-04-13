using Relativity.API;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.SumReporting;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;

namespace Relativity.Sync.Tests.Unit.Executors.SumReporting
{
	[TestFixture]
	internal class NonDocumentJobStartMetricsExecutorTests
    {
        private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1;
        private const int _DESTINATION_WORKSPACE_ARTIFACT_ID = 2;

        private const int _NON_DOCUMENT_OBJECT_TYPE_ID = (int)ArtifactType.View;

        private Mock<IAPILog> _loggerMock;
        private Mock<ISyncMetrics> _syncMetricsMock;

        private Mock<IFieldMappingSummary> _fieldMappingSummaryFake;
        private Mock<IObjectManager> _objectManagerFake;

        private Mock<ISourceServiceFactoryForUser> _serviceFactory;

        private Mock<IObjectTypeManager> _objectTypeManagerMock;
        private Mock<INonDocumentJobStartMetricsConfiguration> _configurationFake;

        private NonDocumentJobStartMetricsExecutor _sut;

		[SetUp]
		public void SetUp()
        {
            _loggerMock = new Mock<IAPILog>();

            _syncMetricsMock = new Mock<ISyncMetrics>();

            _fieldMappingSummaryFake = new Mock<IFieldMappingSummary>();

            _objectManagerFake = new Mock<IObjectManager>(MockBehavior.Strict);
            _objectManagerFake.Setup(x => x.Dispose());

            _serviceFactory = new Mock<ISourceServiceFactoryForUser>();
            _serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>())
                .ReturnsAsync(_objectManagerFake.Object);

            _objectTypeManagerMock = new Mock<IObjectTypeManager>();

			_serviceFactory.Setup(x => x.CreateProxyAsync<IObjectTypeManager>())
                .ReturnsAsync(_objectTypeManagerMock.Object);

			_configurationFake = new Mock<INonDocumentJobStartMetricsConfiguration>();
			_configurationFake.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ARTIFACT_ID);
			_configurationFake.SetupGet(x => x.DestinationWorkspaceArtifactId).Returns(_DESTINATION_WORKSPACE_ARTIFACT_ID);
			_configurationFake.SetupGet(x => x.RdoArtifactTypeId).Returns(_NON_DOCUMENT_OBJECT_TYPE_ID);
            
			_sut = new NonDocumentJobStartMetricsExecutor(
                _serviceFactory.Object,
				_syncMetricsMock.Object,
				_fieldMappingSummaryFake.Object,
                _loggerMock.Object);
        }

		[Test]
		public async Task ExecuteAsync_ShouldReportJobStartMetric()
		{
            // Act
			await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			_syncMetricsMock.Verify(x => x.Send(It.Is<NonDocumentJobStartMetric>(m => 
				m.Type == TelemetryConstants.PROVIDER_NAME &&
				m.FlowType == TelemetryConstants.FLOW_TYPE_VIEW_NON_DOCUMENT_OBJECTS)));
		}


        [Test]
		public async Task ExecuteAsync_ShouldReportRetryMetric_WhenRetryFlowIsSelected()
		{
			// Arrange
			_configurationFake.SetupGet(x => x.JobHistoryToRetryId).Returns(100);

			// Act
			await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
            _syncMetricsMock.Verify(x => x.Send(It.Is<NonDocumentJobStartMetric>(m => m.RetryType != null)));
		}

		[Test]
		public void ExecuteAsync_ShouldComplete_WhenObjectManagerThrows()
		{
			// Arrange
            _objectManagerFake
				.Setup(x => x.QuerySlimAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(),
					It.IsAny<int>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new Exception());

			// Act
			Func<Task> action = () => _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None);

			// Assert
			action.Should().NotThrow();
		}

		[Test]
		public void ExecuteAsync_ShouldComplete_WhenFieldManagerThrows()
		{
			// Arrange
            _fieldMappingSummaryFake.Setup(x => x.GetFieldsMappingSummaryAsync(It.IsAny<CancellationToken>()))
				.ThrowsAsync(new Exception());

			// Act
			Func<Task> action = () => _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None);

			// Assert
			action.Should().NotThrow();
		}


        [Test]
        public async Task ExecuteAsync_ShouldLogFieldsMappingDetails()
        {
			// Arrange
            Dictionary<string, object> summary = new Dictionary<string, object>();
            _fieldMappingSummaryFake.Setup(x => x.GetFieldsMappingSummaryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(summary);

            // Act
            await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            _loggerMock.Verify(x => x.LogInformation("Fields mapping summary: {@fieldsMappingSummary}", It.IsAny<Dictionary<string, object>>()));
        }


        //[Test]
        //public async Task ExecuteAsync_ShouldReportApplicationName()
        //{
        //    List<DisplayableObjectIdentifier> displayableObjectIdentifiers = new List<DisplayableObjectIdentifier>
        //    {
        //        new DisplayableObjectIdentifier
        //        {
        //            Name = "Adler Sieben 1",
        //        },
        //        new DisplayableObjectIdentifier
        //        {
        //            Name = "Adler Sieben 2",
        //        }
        //    };

        //    ObjectTypeResponse objectTypeResponse = new ObjectTypeResponse
        //    {
        //        ArtifactTypeID = _NON_DOCUMENT_OBJECT_TYPE_ID,
        //        RelativityApplications =
        //            new SecurableList<DisplayableObjectIdentifier>(false, displayableObjectIdentifiers)
        //    };

        //    _objectTypeManagerMock.Setup(x => x.ReadAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _NON_DOCUMENT_OBJECT_TYPE_ID))
        //        .Returns(Task.FromResult(objectTypeResponse));

        //}

        [Test]
		public async Task ExecuteAsync_ShouldReportJobResumeMetric_WhenResuming()
		{
			// Arrange
			_configurationFake.SetupGet(x => x.Resuming).Returns(true);

			// Act
			await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
            _syncMetricsMock.Verify(x => x.Send(It.Is<JobResumeMetric>(metric =>
				metric.Type == TelemetryConstants.PROVIDER_NAME)), Times.Once);
            _syncMetricsMock.Verify(x => x.Send(It.IsAny<NonDocumentJobStartMetric>()), Times.Never);

            _loggerMock.Verify(x => x.LogInformation("Fields map configuration summary: {@summary}", It.IsAny<Dictionary<string, object>>()), Times.Never);
		}
    }
}
