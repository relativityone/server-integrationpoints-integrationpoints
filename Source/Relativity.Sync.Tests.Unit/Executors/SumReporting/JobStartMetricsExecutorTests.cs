using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.SumReporting;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit.Executors.SumReporting
{
	[TestFixture]
	internal class JobStartMetricsExecutorTests
	{
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1;
		private const int _DESTINATION_WORKSPACE_ARTIFACT_ID = 2;


		private JobStartMetricsExecutor _sut;

		private Mock<ISyncMetrics> _syncMetricsMock;
		private Mock<ISumReporterConfiguration> _sumReporterConfigurationFake;
		private Mock<ISyncLog> _syncLoggerMock;
		private Mock<IFieldManager> _fieldManagerMock;
		private Mock<ISourceServiceFactoryForUser> _serviceFactoryMock;
		private Mock<ISerializer> _serializerMock;
		private Mock<IObjectManager> _objectManagerMock;
		private Mock<IPipelineSelector> _pipelineSelectorFake;

		private static readonly ISyncPipeline[] DocumentTypePipelines = new ISyncPipeline[]
		{
			new SyncDocumentRunPipeline(),
			new SyncDocumentRetryPipeline()
		};

		// TODO: REL-465065
		private static ISyncPipeline[] _imageTypePipelines = new ISyncPipeline[]
		{
		//	new SyncImageRunPipeline(),
		//	new SyncImageRetryPipeline()
		};

		[SetUp]
		public void SetUp()
		{
			_syncMetricsMock = new Mock<ISyncMetrics>();
			_sumReporterConfigurationFake = new Mock<ISumReporterConfiguration>();
			_syncLoggerMock = new Mock<ISyncLog>();
			_fieldManagerMock = new Mock<IFieldManager>();

			_serviceFactoryMock = new Mock<ISourceServiceFactoryForUser>();
			_serializerMock = new Mock<ISerializer>();
			_objectManagerMock = new Mock<IObjectManager>();
			_pipelineSelectorFake = new Mock<IPipelineSelector>();

			_sumReporterConfigurationFake.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ARTIFACT_ID);
			_sumReporterConfigurationFake.SetupGet(x => x.DestinationWorkspaceArtifactId).Returns(_DESTINATION_WORKSPACE_ARTIFACT_ID);


			_serializerMock.Setup(x => x.Serialize(It.IsAny<object>())).Returns((object x) =>
			{
				var serializer = new JSONSerializer();
				return serializer.Serialize(x);
			});

			_serviceFactoryMock.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManagerMock.Object);

			_fieldManagerMock.Setup(x => x.GetMappedDocumentFieldsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<FieldInfoDto>
			{
				new FieldInfoDto(SpecialFieldType.None,"Control Number", "Control Number", true, true){RelativityDataType = RelativityDataType.FixedLengthText}
			});

			_pipelineSelectorFake.Setup(x => x.GetPipeline()).Returns(DocumentTypePipelines[0]);

			_objectManagerMock
				.Setup(x => x.QuerySlimAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new QueryResultSlim
				{
					Objects = new List<RelativityObjectSlim>
						{
							new RelativityObjectSlim
							{
								ArtifactID = 1, Values = new List<object>{"Control Number", false}
							}
						}
				});

			_sut = new JobStartMetricsExecutor(_syncLoggerMock.Object, _syncMetricsMock.Object, _pipelineSelectorFake.Object, _fieldManagerMock.Object, _serviceFactoryMock.Object, _serializerMock.Object);
		}

		[Test]
		public async Task ExecuteAsyncReportsMetricAndCompletesSuccessfullyTest()
		{
			// Act
			ExecutionResult actualResult = await _sut.ExecuteAsync(_sumReporterConfigurationFake.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			actualResult.Status.Should().Be(ExecutionStatus.Completed);

			_syncMetricsMock.Verify(x => x.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.JOB_START_TYPE, TelemetryConstants.PROVIDER_NAME), Times.Once);
		}

		[Test]
		public async Task ExecuteAsync_ShouldSendStartRetryMetric_WhenJobHasBeenRetried()
		{
			// Arrange
			_sumReporterConfigurationFake.Setup(x => x.JobHistoryToRetryId).Returns(It.IsAny<int>());

			// Act
			await _sut.ExecuteAsync(_sumReporterConfigurationFake.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			_syncMetricsMock.Verify(x => x.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.RETRY_JOB_START_TYPE, TelemetryConstants.PROVIDER_NAME), Times.Once);
		}

		[TestCaseSource(nameof(DocumentTypePipelines))]
		public async Task ExecuteAsync_ShouldLogSavedSearchNativesAndMetadataFlowType(ISyncPipeline syncPipeline)
		{
			// Arrange
			_pipelineSelectorFake.Setup(x => x.GetPipeline()).Returns(syncPipeline);

			// Act
			await _sut.ExecuteAsync(_sumReporterConfigurationFake.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			_syncMetricsMock.Verify(x => x.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.FLOW_TYPE, TelemetryConstants.FLOW_TYPE_SAVED_SEARCH_NATIVES_AND_METADATA), Times.Once);
		}

		[TestCaseSource(nameof(_imageTypePipelines))]
		public async Task ExecuteAsync_ShouldLogSavedSearchImagesFlowType(ISyncPipeline syncPipeline)
		{
			// Arrange
			_pipelineSelectorFake.Setup(x => x.GetPipeline()).Returns(syncPipeline);

			// Act
			await _sut.ExecuteAsync(_sumReporterConfigurationFake.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			_syncMetricsMock.Verify(x => x.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.FLOW_TYPE, TelemetryConstants.FLOW_TYPE_SAVED_SEARCH_IMAGES), Times.Once);
		}

		[Test]
		public async Task ExecuteAsync_ShouldLogFieldsMappingDetails()
		{
			// Act
			await _sut.ExecuteAsync(_sumReporterConfigurationFake.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			_syncLoggerMock.Verify(x => x.LogInformation("Fields map configuration summary: {summary}", It.IsAny<string>()));
		}

		[Test]
		public void ExecuteAsync_Should_CompleteSuccessfully_WhenObjectManagerThrows()
		{
			// Arrange
			_objectManagerMock
				.Setup(x => x.QuerySlimAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(),
					It.IsAny<int>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new Exception());

			// Act
			Func<Task> action = async () => await _sut.ExecuteAsync(_sumReporterConfigurationFake.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			action.Should().NotThrow();
		}

		[Test]
		public async Task ExecuteAsync_Should_NotCallObjectManager_IfThereIsNoLongTextField()
		{
			// Act
			await _sut.ExecuteAsync(_sumReporterConfigurationFake.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			_objectManagerMock.Invocations.Count.Should().Be(0);
		}
		
		[Test]
		public void ExecuteAsync_Should_CompleteSuccessfully_WhenFieldManagerThrows()
		{
			// Arrange
		_fieldManagerMock.Setup(x => x.GetMappedDocumentFieldsAsync(It.IsAny<CancellationToken>()))
				.ThrowsAsync(new Exception());

			// Act
			Func<Task> action = async () => await _sut.ExecuteAsync(_sumReporterConfigurationFake.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			action.Should().NotThrow();
		}
		
		[TestCaseSource(nameof(FieldsMappingTestCaseSource))]
		public async Task ExecuteAsync_Should_Log_Correct_FieldsMappingDetails(List<FieldMapDefinitionCase> mapping, string expectedLog)
		{
			// Arrange

			SetupFieldMapping(mapping);

			// Act
			await _sut.ExecuteAsync(_sumReporterConfigurationFake.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			_syncLoggerMock.Verify(x => x.LogInformation("Fields map configuration summary: {summary}", expectedLog));
		}

		public static IEnumerable<TestCaseData> FieldsMappingTestCaseSource()
		{
			const string extractedTextFieldName = "Extracted Text";

			yield return new TestCaseData(
				new List<FieldMapDefinitionCase>
				{
					new FieldMapDefinitionCase{SourceFieldName = extractedTextFieldName, DestinationFieldName = extractedTextFieldName, DataType = RelativityDataType.LongText},
				},
				"{\"FieldMapping\":{\"LongText\":1},\"ExtractedText\":{\"Source\":{\"ArtifactId\":1,\"DataGridEnabled\":false},\"Destination\":{\"ArtifactId\":2,\"DataGridEnabled\":false}},\"LongText\":[]}"
				)
			{ TestName = "{m}(ExtractedTextDataGridSource=disable, ExtractedTextDataGridDestination=disabled)" };

			yield return new TestCaseData(
				new List<FieldMapDefinitionCase>
				{
					new FieldMapDefinitionCase{SourceFieldName = extractedTextFieldName, DestinationFieldName = extractedTextFieldName, DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = true, DestinationFieldDataGridEnabled = false}
				},
				"{\"FieldMapping\":{\"LongText\":1},\"ExtractedText\":{\"Source\":{\"ArtifactId\":1,\"DataGridEnabled\":true},\"Destination\":{\"ArtifactId\":2,\"DataGridEnabled\":false}},\"LongText\":[]}"
				)
			{ TestName = "{m}(ExtractedTextDataGridSource=enabled, ExtractedTextDataGridDestination=disabled)" };

			yield return new TestCaseData(
				new List<FieldMapDefinitionCase>
				{
					new FieldMapDefinitionCase{SourceFieldName = extractedTextFieldName, DestinationFieldName = extractedTextFieldName, DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = false, DestinationFieldDataGridEnabled = true}
				},
				"{\"FieldMapping\":{\"LongText\":1},\"ExtractedText\":{\"Source\":{\"ArtifactId\":1,\"DataGridEnabled\":false},\"Destination\":{\"ArtifactId\":2,\"DataGridEnabled\":true}},\"LongText\":[]}"
			)
			{ TestName = "{m}(ExtractedTextDataGridSource=disabled, ExtractedTextDataGridDestination=enabled)" };

			yield return new TestCaseData(
				new List<FieldMapDefinitionCase>
				{
					new FieldMapDefinitionCase{SourceFieldName = extractedTextFieldName, DestinationFieldName = extractedTextFieldName, DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = true, DestinationFieldDataGridEnabled = true}
				},
				"{\"FieldMapping\":{\"LongText\":1},\"ExtractedText\":{\"Source\":{\"ArtifactId\":1,\"DataGridEnabled\":true},\"Destination\":{\"ArtifactId\":2,\"DataGridEnabled\":true}},\"LongText\":[]}"
				)
			{ TestName = "{m}(ExtractedTextDataGridSource=enabled, ExtractedTextDataGridDestination=enabled)" };

			yield return new TestCaseData(
					new List<FieldMapDefinitionCase>
					{
						new FieldMapDefinitionCase{SourceFieldName = "fixed-length", DestinationFieldName = "fixed-length", DataType = RelativityDataType.FixedLengthText},
						new FieldMapDefinitionCase{SourceFieldName = "fixed-length 2", DestinationFieldName = "fixed-length 2", DataType = RelativityDataType.FixedLengthText},
						new FieldMapDefinitionCase{SourceFieldName = "number", DestinationFieldName = "number destination", DataType = RelativityDataType.WholeNumber},
						new FieldMapDefinitionCase{SourceFieldName = "randomName", DestinationFieldName = "CommandoreBomardiero", DataType = RelativityDataType.FixedLengthText},
						new FieldMapDefinitionCase{SourceFieldName = "AdlerSieben", DestinationFieldName = "SeniorGordo", DataType = RelativityDataType.FixedLengthText},
						new FieldMapDefinitionCase{SourceFieldName = "1", DestinationFieldName = "1", DataType = RelativityDataType.Currency},
						new FieldMapDefinitionCase{SourceFieldName = "2", DestinationFieldName = "2", DataType = RelativityDataType.Currency},
						new FieldMapDefinitionCase{SourceFieldName = "3", DestinationFieldName = "3", DataType = RelativityDataType.YesNo},
						new FieldMapDefinitionCase{SourceFieldName = "4", DestinationFieldName = "4", DataType = RelativityDataType.YesNo},
						new FieldMapDefinitionCase{SourceFieldName = "5", DestinationFieldName = "5", DataType = RelativityDataType.YesNo},
						new FieldMapDefinitionCase{SourceFieldName = "6", DestinationFieldName = "6", DataType = RelativityDataType.YesNo},
					},
					"{\"FieldMapping\":{\"FixedLengthText\":4,\"WholeNumber\":1,\"Currency\":2,\"YesNo\":4},\"ExtractedText\":null,\"LongText\":[]}"
				)
				{ TestName = "{m}(CountingTypes)" };

			yield return new TestCaseData(
					new List<FieldMapDefinitionCase>
					{
						new FieldMapDefinitionCase{SourceFieldName = extractedTextFieldName, DestinationFieldName = extractedTextFieldName, DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = true, DestinationFieldDataGridEnabled = true},
						new FieldMapDefinitionCase{SourceFieldName = "long text1", DestinationFieldName = "long text1 dest", DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = false, DestinationFieldDataGridEnabled = true},
						new FieldMapDefinitionCase{SourceFieldName = "long text2", DestinationFieldName = "long text2", DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = true, DestinationFieldDataGridEnabled = false}
					},
					"{\"FieldMapping\":{\"LongText\":3},\"ExtractedText\":{\"Source\":{\"ArtifactId\":1,\"DataGridEnabled\":true},\"Destination\":{\"ArtifactId\":4,\"DataGridEnabled\":true}},\"LongText\":[{\"Source\":{\"ArtifactId\":2,\"DataGridEnabled\":false},\"Destination\":{\"ArtifactId\":5,\"DataGridEnabled\":true}},{\"Source\":{\"ArtifactId\":3,\"DataGridEnabled\":true},\"Destination\":{\"ArtifactId\":6,\"DataGridEnabled\":false}}]}"
				)
				{ TestName = "{m}(LongText)" };

			yield return new TestCaseData(
					new List<FieldMapDefinitionCase>
					{
						new FieldMapDefinitionCase{SourceFieldName = extractedTextFieldName, DestinationFieldName = extractedTextFieldName, DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = true, DestinationFieldDataGridEnabled = true},
						new FieldMapDefinitionCase{SourceFieldName = "long text1", DestinationFieldName = "long text1 dest", DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = false, DestinationFieldDataGridEnabled = true},
						new FieldMapDefinitionCase{SourceFieldName = "long text2", DestinationFieldName = "long text2", DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = true, DestinationFieldDataGridEnabled = false},
						new FieldMapDefinitionCase{SourceFieldName = "Native path", DestinationFieldName = "Native path", DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = true, DestinationFieldDataGridEnabled = false, SpecialFieldType = SpecialFieldType.NativeFileLocation},
						new FieldMapDefinitionCase{SourceFieldName = "Native location", DestinationFieldName = "Native location", DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = true, DestinationFieldDataGridEnabled = false, SpecialFieldType = SpecialFieldType.NativeFileFilename}
					},
					"{\"FieldMapping\":{\"LongText\":5},\"ExtractedText\":{\"Source\":{\"ArtifactId\":1,\"DataGridEnabled\":true},\"Destination\":{\"ArtifactId\":6,\"DataGridEnabled\":true}},\"LongText\":[{\"Source\":{\"ArtifactId\":2,\"DataGridEnabled\":false},\"Destination\":{\"ArtifactId\":7,\"DataGridEnabled\":true}},{\"Source\":{\"ArtifactId\":3,\"DataGridEnabled\":true},\"Destination\":{\"ArtifactId\":8,\"DataGridEnabled\":false}},{\"Source\":{\"ArtifactId\":4,\"DataGridEnabled\":true},\"Destination\":{\"ArtifactId\":9,\"DataGridEnabled\":false}},{\"Source\":{\"ArtifactId\":5,\"DataGridEnabled\":true},\"Destination\":{\"ArtifactId\":10,\"DataGridEnabled\":false}}]}"
				)
				{ TestName = "{m}(ShouldLogSpecialFieldsWhenTheyHaveBeenMapped)" };
		}

		private void SetupFieldMapping(IEnumerable<FieldMapDefinitionCase> mapping)
		{
			int artifactIdCounter = 1;
			_objectManagerMock
				.Setup(x => x.QuerySlimAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new QueryResultSlim
				{
					Objects = mapping.Select(x =>
						new RelativityObjectSlim
						{
							ArtifactID = artifactIdCounter++,
							Values = new List<object> { x.SourceFieldName, x.SourceFieldDataGridEnabled }
						}
					).ToList()
				});

			_objectManagerMock
				.Setup(x => x.QuerySlimAsync(_DESTINATION_WORKSPACE_ARTIFACT_ID, It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new QueryResultSlim
				{
					Objects = mapping.Select(x =>
						new RelativityObjectSlim
						{
							ArtifactID = artifactIdCounter++,
							Values = new List<object> { x.DestinationFieldName, x.DestinationFieldDataGridEnabled }
						}
					).ToList()
				});


			_fieldManagerMock.Setup(x => x.GetMappedDocumentFieldsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mapping.Select(x =>
					new FieldInfoDto(x.SpecialFieldType, x.SourceFieldName, x.DestinationFieldName, true, true) { RelativityDataType = x.DataType }
				).ToList);
		}

		internal class FieldMapDefinitionCase
		{
			public string SourceFieldName { get; set; }
			public string DestinationFieldName { get; set; }
			public RelativityDataType DataType { get; set; }
			public bool SourceFieldDataGridEnabled { get; set; }
			public bool DestinationFieldDataGridEnabled { get; set; }
			public SpecialFieldType SpecialFieldType { get; set; } = SpecialFieldType.None;
		}
	}
}