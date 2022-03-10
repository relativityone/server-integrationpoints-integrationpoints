using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.SumReporting;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors.SumReporting
{
	[TestFixture]
	internal class NonDocumentJobStartMetricsExecutorTests : JobStartMetricsExecutorTestsBase
	{
        private Mock<IObjectTypeManager> _objectTypeManagerMock;
        private Mock<INonDocumentJobStartMetricsConfiguration> _configurationFake;

        private NonDocumentJobStartMetricsExecutor _sut;
        private const int _NON_DOCUMENT_OBJECT_TYPE_ID = (int)ArtifactType.View;

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
            _objectTypeManagerMock = new Mock<IObjectTypeManager>();

			ServiceFactory.Setup(x => x.CreateProxyAsync<IObjectTypeManager>())
                .ReturnsAsync(_objectTypeManagerMock.Object);

			_configurationFake = new Mock<INonDocumentJobStartMetricsConfiguration>();
			_configurationFake.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(SOURCE_WORKSPACE_ARTIFACT_ID);
			_configurationFake.SetupGet(x => x.DestinationWorkspaceArtifactId).Returns(DESTINATION_WORKSPACE_ARTIFACT_ID);
			_configurationFake.SetupGet(x => x.RdoArtifactTypeId).Returns(_NON_DOCUMENT_OBJECT_TYPE_ID);
            
            PrepareTestData();

			_sut = new NonDocumentJobStartMetricsExecutor(
                ServiceFactory.Object,
				SyncLogMock.Object,
				SyncMetricsMock.Object,
				FieldManagerFake.Object);
        }

		[Test]
		public async Task ExecuteAsync_ShouldReportJobStartMetric()
		{
            // Act
			await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			SyncMetricsMock.Verify(x => x.Send(It.Is<NonDocumentJobStartMetric>(m => 
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
            SyncMetricsMock.Verify(x => x.Send(It.Is<NonDocumentJobStartMetric>(m => m.RetryType != null)));
		}

		[Test]
		public async Task ExecuteAsync_ShouldLogFieldsMappingDetails()
		{
			// Arrange
			FieldManagerFake.Setup(x => x.GetMappedFieldsAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(new List<FieldInfoDto>());

			// Act
			await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			SyncLogMock.Verify(x => x.LogInformation("Fields map configuration summary: {@summary}", It.IsAny<Dictionary<string, object>>()));
		}

		[Test]
		public void ExecuteAsync_ShouldComplete_WhenObjectManagerThrows()
		{
			// Arrange
            ObjectManagerFake
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
            FieldManagerFake.Setup(x => x.GetMappedFieldsAsync(It.IsAny<CancellationToken>()))
				.ThrowsAsync(new Exception());

			// Act
			Func<Task> action = () => _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None);

			// Assert
			action.Should().NotThrow();
		}


		[TestCaseSource(nameof(FieldsMappingTestCaseSource))]
		public async Task ExecuteAsync_ShouldLogFieldsMappingDetails(List<FieldMapDefinitionCase> mapping, Dictionary<string, object> expectedLog)
		{
			// Arrange
			SetupFieldMapping(mapping);

			// Act
			await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			Func<Dictionary<string, object>, bool> verify = actual =>
			{
				CollectionAssert.AreEquivalent(actual, expectedLog);
				return true;
			};
            SyncLogMock.Verify(x => x.LogInformation("Fields map configuration summary: {@summary}", It.Is<Dictionary<string, object>>(actual => verify(actual))));
		}

		[Test]
		public async Task ExecuteAsync_ShouldReportJobResumeMetric_WhenResuming()
		{
			// Arrange
			_configurationFake.SetupGet(x => x.Resuming).Returns(true);

			// Act
			await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
            SyncMetricsMock.Verify(x => x.Send(It.Is<JobResumeMetric>(metric =>
				metric.Type == TelemetryConstants.PROVIDER_NAME)), Times.Once);
            SyncMetricsMock.Verify(x => x.Send(It.IsAny<NonDocumentJobStartMetric>()), Times.Never);

            SyncLogMock.Verify(x => x.LogInformation("Fields map configuration summary: {@summary}", It.IsAny<Dictionary<string, object>>()), Times.Never);
		}

		

		private void PrepareTestData()
		{
			List<DisplayableObjectIdentifier> displayableObjectIdentifiers = new List<DisplayableObjectIdentifier>
			{
				new DisplayableObjectIdentifier
				{
					Name = "Adler Sieben 1",
				},
				new DisplayableObjectIdentifier
				{
					Name = "Adler Sieben 2",
				}
			};
			ObjectTypeResponse objectTypeResponse = new ObjectTypeResponse
			{
				ArtifactTypeID = _NON_DOCUMENT_OBJECT_TYPE_ID,
				RelativityApplications =
					new SecurableList<DisplayableObjectIdentifier>(false, displayableObjectIdentifiers)
			};
			_objectTypeManagerMock.Setup(x => x.ReadAsync(SOURCE_WORKSPACE_ARTIFACT_ID, _NON_DOCUMENT_OBJECT_TYPE_ID))
				.Returns(Task.FromResult(objectTypeResponse));

			IList<FieldInfoDto> fieldsDtos = new List<FieldInfoDto>
			{
				new FieldInfoDto(SpecialFieldType.None, "source Field 1", "destination Field 1",
					false, false)
				{
					RelativityDataType = RelativityDataType.LongText
				},
				new FieldInfoDto(SpecialFieldType.None, "source Field 2", "destination Field 2",
					false, false)
				{
					RelativityDataType = RelativityDataType.LongText
				},
				new FieldInfoDto(SpecialFieldType.None, "source Field 3", "destination Field 3",
					false, false)
				{
					RelativityDataType = RelativityDataType.LongText
				}
			};

            FieldManagerFake.Setup(x => x.GetMappedFieldsAsync(It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(fieldsDtos));

			QueryResultSlim queryResultSlim = new QueryResultSlim
			{
				Objects = new List<RelativityObjectSlim>
				{
					new RelativityObjectSlim
					{
						Values = new List<object>
						{
							"ValuesId",
							"value 1",
							"value 2",
							"value 3",
						}
					}
				}
			};

            ObjectManagerFake.Setup(x => x.QuerySlimAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(),
					fieldsDtos.Count, It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(queryResultSlim));
		}
    }
}
