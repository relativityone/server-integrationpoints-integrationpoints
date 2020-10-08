using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Collections;
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
using Relativity.Transfer;

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

		// TODO: REL-465065
		private static readonly TestCaseData[] LogFlowTypeTestCases =
		{
			new TestCaseData(new SyncDocumentRunPipeline(), TelemetryConstants.FLOW_TYPE_SAVED_SEARCH_NATIVES_AND_METADATA),
			new TestCaseData(new SyncDocumentRetryPipeline(), TelemetryConstants.FLOW_TYPE_SAVED_SEARCH_NATIVES_AND_METADATA)
		//	new TestCaseData(new SyncImageRunPipeline(), TelemetryConstants.FLOW_TYPE_SAVED_SEARCH_IMAGES),
		//	new TestCaseData(new SyncImageRetryPipeline(), TelemetryConstants.FLOW_TYPE_SAVED_SEARCH_IMAGES)
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

			ISyncPipeline defaultPipeline = new SyncDocumentRunPipeline();
			_pipelineSelectorFake.Setup(x => x.GetPipeline()).Returns(defaultPipeline);

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

			_sut = new JobStartMetricsExecutor(_syncLoggerMock.Object, _syncMetricsMock.Object, _pipelineSelectorFake.Object, _fieldManagerMock.Object, _serviceFactoryMock.Object);
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

		[TestCaseSource(nameof(LogFlowTypeTestCases))]
		public async Task ExecuteAsync_ShouldLogCorrectFlowType(ISyncPipeline syncPipeline, string flowType)
		{
			// Arrange
			_pipelineSelectorFake.Setup(x => x.GetPipeline()).Returns(syncPipeline);

			// Act
			await _sut.ExecuteAsync(_sumReporterConfigurationFake.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			_syncMetricsMock.Verify(x => x.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.FLOW_TYPE, flowType), Times.Once);
		}

		[Test]
		public async Task ExecuteAsync_ShouldLogFieldsMappingDetails()
		{
			// Act
			await _sut.ExecuteAsync(_sumReporterConfigurationFake.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			_syncLoggerMock.Verify(x => x.LogInformation("Fields map configuration summary: {@summary}", It.IsAny<Dictionary<string, object>>()));
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
			Func<Task> action = () => _sut.ExecuteAsync(_sumReporterConfigurationFake.Object, CancellationToken.None);

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
			Func<Task> action = () => _sut.ExecuteAsync(_sumReporterConfigurationFake.Object, CancellationToken.None);

			// Assert
			action.Should().NotThrow();
		}
		
		[TestCaseSource(nameof(FieldsMappingTestCaseSource))]
		public async Task ExecuteAsync_Should_Log_Correct_FieldsMappingDetails(List<FieldMapDefinitionCase> mapping, Dictionary<string, object> expectedLog)
		{
			// Arrange
			SetupFieldMapping(mapping);

			// Act
			await _sut.ExecuteAsync(_sumReporterConfigurationFake.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			Func<Dictionary<string, object>, bool> verify = actual =>
			{
				CollectionAssert.AreEquivalent(actual, expectedLog);
				return true;
			};
			_syncLoggerMock.Verify(x => x.LogInformation("Fields map configuration summary: {@summary}", It.Is<Dictionary<string, object>>(actual => verify(actual))));
		}

		public static IEnumerable<TestCaseData> FieldsMappingTestCaseSource()
		{
			const string extractedTextFieldName = "Extracted Text";

			yield return new TestCaseData(
				new List<FieldMapDefinitionCase>
				{
						new FieldMapDefinitionCase
						{
							SourceFieldName = extractedTextFieldName, DestinationFieldName = extractedTextFieldName,
							DataType = RelativityDataType.LongText
				},
					},
					PrepareSummaryForExtractedTextWithDisabledDataGrid()
				)
				{TestName = "ExtractedTextDataGrid(Source=disable, Destination=disabled)"};

			yield return new TestCaseData(
				new List<FieldMapDefinitionCase>
				{
						new FieldMapDefinitionCase
						{
							SourceFieldName = extractedTextFieldName, DestinationFieldName = extractedTextFieldName,
							DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = true,
							DestinationFieldDataGridEnabled = false
						}
				},
					PrepareSummaryForExtractedTextWithEnabledDataGridInSource()
				)
			{ TestName = "{m}(ExtractedTextDataGridSource=enabled, ExtractedTextDataGridDestination=disabled)" };

			yield return new TestCaseData(
				new List<FieldMapDefinitionCase>
				{
						new FieldMapDefinitionCase
						{
							SourceFieldName = extractedTextFieldName, DestinationFieldName = extractedTextFieldName,
							DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = false,
							DestinationFieldDataGridEnabled = true
						}
				},
					PrepareSummaryForExtractedTextWithEnabledDataGridInDestination()
			)
			{ TestName = "{m}(ExtractedTextDataGridSource=disabled, ExtractedTextDataGridDestination=enabled)" };

			yield return new TestCaseData(
				new List<FieldMapDefinitionCase>
				{
						new FieldMapDefinitionCase
						{
							SourceFieldName = extractedTextFieldName, DestinationFieldName = extractedTextFieldName,
							DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = true,
							DestinationFieldDataGridEnabled = true
						}
				},
					PrepareSummaryForExtractedTextWithDataGridEnabledBothInSourceAndDestination()
				)
			{ TestName = "{m}(ExtractedTextDataGridSource=enabled, ExtractedTextDataGridDestination=enabled)" };

			yield return new TestCaseData(
					new List<FieldMapDefinitionCase>
					{
						new FieldMapDefinitionCase
						{
							SourceFieldName = "fixed-length", DestinationFieldName = "fixed-length",
							DataType = RelativityDataType.FixedLengthText
					},
						new FieldMapDefinitionCase
						{
							SourceFieldName = "fixed-length 2", DestinationFieldName = "fixed-length 2",
							DataType = RelativityDataType.FixedLengthText
						},
						new FieldMapDefinitionCase
						{
							SourceFieldName = "number", DestinationFieldName = "number destination",
							DataType = RelativityDataType.WholeNumber
						},
						new FieldMapDefinitionCase
						{
							SourceFieldName = "randomName", DestinationFieldName = "CommandoreBomardiero",
							DataType = RelativityDataType.FixedLengthText
						},
						new FieldMapDefinitionCase
						{
							SourceFieldName = "AdlerSieben", DestinationFieldName = "SeniorGordo",
							DataType = RelativityDataType.FixedLengthText
						},
						new FieldMapDefinitionCase
							{SourceFieldName = "1", DestinationFieldName = "1", DataType = RelativityDataType.Currency},
						new FieldMapDefinitionCase
							{SourceFieldName = "2", DestinationFieldName = "2", DataType = RelativityDataType.Currency},
						new FieldMapDefinitionCase
							{SourceFieldName = "3", DestinationFieldName = "3", DataType = RelativityDataType.YesNo},
						new FieldMapDefinitionCase
							{SourceFieldName = "4", DestinationFieldName = "4", DataType = RelativityDataType.YesNo},
						new FieldMapDefinitionCase
							{SourceFieldName = "5", DestinationFieldName = "5", DataType = RelativityDataType.YesNo},
						new FieldMapDefinitionCase
							{SourceFieldName = "6", DestinationFieldName = "6", DataType = RelativityDataType.YesNo},
					},
					PrepareSummaryForCountingTypes()
				)
				{TestName = "Counting Types"};

			yield return new TestCaseData(
					new List<FieldMapDefinitionCase>
					{
						new FieldMapDefinitionCase
						{
							SourceFieldName = extractedTextFieldName, DestinationFieldName = extractedTextFieldName,
							DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = true,
							DestinationFieldDataGridEnabled = true
								},
						new FieldMapDefinitionCase
								{
							SourceFieldName = "long text1", DestinationFieldName = "long text1 dest",
							DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = false,
							DestinationFieldDataGridEnabled = true
								},
						new FieldMapDefinitionCase
								{
							SourceFieldName = "long text2", DestinationFieldName = "long text2",
							DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = true,
							DestinationFieldDataGridEnabled = false
								}
						},
					PrepareSummaryForLongText()
				)
				{ TestName = "{m}(CountingTypes)" };

			yield return new TestCaseData(
					new List<FieldMapDefinitionCase>
					{
						new FieldMapDefinitionCase
						{
							SourceFieldName = extractedTextFieldName, DestinationFieldName = extractedTextFieldName,
							DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = true,
							DestinationFieldDataGridEnabled = true
					},
						new FieldMapDefinitionCase
						{
							SourceFieldName = "long text1", DestinationFieldName = "long text1 dest",
							DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = false,
							DestinationFieldDataGridEnabled = true
						},
						new FieldMapDefinitionCase
						{
							SourceFieldName = "long text2", DestinationFieldName = "long text2",
							DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = true,
							DestinationFieldDataGridEnabled = false
						},
						new FieldMapDefinitionCase
						{
							SourceFieldName = "Native path", DestinationFieldName = "Native path",
							DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = true,
							DestinationFieldDataGridEnabled = false,
							SpecialFieldType = SpecialFieldType.NativeFileLocation
						},
						new FieldMapDefinitionCase
						{
							SourceFieldName = "Native location", DestinationFieldName = "Native location",
							DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = true,
							DestinationFieldDataGridEnabled = false,
							SpecialFieldType = SpecialFieldType.NativeFileFilename
						}
					},
					PrepareSummaryForNotLoggingSpecialFields()
				)
				{TestName = "Should not log special fields"};
		}

		private static Dictionary<string, object> PrepareSummaryForNotLoggingSpecialFields()
					{
			return new Dictionary<string, object>()
						{
				{
							"FieldMapping", new Dictionary<string, int>()
							{
								{
									"LongText", 3
								}
							}
						},
						{
							"ExtractedText", new Dictionary<string, object>()
							{
								{
									"Source", new Dictionary<string, object>()
									{
										{"ArtifactId", 1},
										{"DataGridEnabled", true}
									}
								},
								{
									"Destination", new Dictionary<string, object>()
									{
								{"ArtifactId", 6},
								{"DataGridEnabled", true}
							}
						}
					}
				},
				{
					"LongText", new Dictionary<string, Dictionary<string, object>>[]
					{
						new Dictionary<string, Dictionary<string, object>>()
						{
							{
								"Source", new Dictionary<string, object>()
								{
									{"ArtifactId", 2},
									{"DataGridEnabled", false}
								}
							},
							{
								"Destination", new Dictionary<string, object>()
								{
									{"ArtifactId", 7},
									{"DataGridEnabled", true}
								}
							}
						},
						new Dictionary<string, Dictionary<string, object>>()
						{
							{
								"Source", new Dictionary<string, object>()
								{
									{"ArtifactId", 3},
									{"DataGridEnabled", true}
								}
							},
							{
								"Destination", new Dictionary<string, object>()
								{
									{"ArtifactId", 8},
									{"DataGridEnabled", false}
								}
							}
						}
					}
				}
			};
		}

		private static Dictionary<string, object> PrepareSummaryForLongText()
		{
			return new Dictionary<string, object>()
			{
				{
					"FieldMapping", new Dictionary<string, int>()
					{
						{
							"LongText", 3
						}
					}
				},
				{
					"ExtractedText", new Dictionary<string, object>()
					{
						{
							"Source", new Dictionary<string, object>()
							{
								{"ArtifactId", 1},
								{"DataGridEnabled", true}
							}
						},
						{
							"Destination", new Dictionary<string, object>()
							{
										{"ArtifactId", 4},
										{"DataGridEnabled", true}
									}
								}
							}
						},
						{
							"LongText", new Dictionary<string, Dictionary<string, object>>[]
							{
								new Dictionary<string, Dictionary<string, object>>()
								{
									{
										"Source", new Dictionary<string, object>()
										{
											{"ArtifactId", 2},
											{"DataGridEnabled", false}
										}
									},
									{
										"Destination", new Dictionary<string, object>()
										{
											{"ArtifactId", 5},
											{"DataGridEnabled", true}
										}
									}
								},
								new Dictionary<string, Dictionary<string, object>>()
								{
									{
										"Source", new Dictionary<string, object>()
										{
											{"ArtifactId", 3},
											{"DataGridEnabled", true}
										}
									},
									{
										"Destination", new Dictionary<string, object>()
										{
											{"ArtifactId", 6},
											{"DataGridEnabled", false}
										}
									}
								}
							}
						}
			};
					}
				)
				{ TestName = "{m}(LongText)" };

		private static Dictionary<string, object> PrepareSummaryForCountingTypes()
					{
			return new Dictionary<string, object>()
						{
						{
					"FieldMapping", new Dictionary<string, int>()
					{
						{
							"FixedLengthText", 4
						},
						{
							"WholeNumber", 1
						},
						{
							"Currency", 2
						},
						{
							"YesNo", 4
						}
					}
					},
					{
					"ExtractedText", null
				},
						{
					"LongText", new Dictionary<string, Dictionary<string, object>>[0]
				}
			};
		}

		private static Dictionary<string, object> PrepareSummaryForExtractedTextWithDataGridEnabledBothInSourceAndDestination()
		{
			return new Dictionary<string, object>()
			{
				{
							"FieldMapping", new Dictionary<string, int>()
							{
								{
							"LongText", 1
								}
							}
						},
						{
							"ExtractedText", new Dictionary<string, object>()
							{
								{
									"Source", new Dictionary<string, object>()
									{
										{"ArtifactId", 1},
										{"DataGridEnabled", true}
									}
								},
								{
									"Destination", new Dictionary<string, object>()
									{
								{"ArtifactId", 2},
										{"DataGridEnabled", true}
									}
								}
							}
						},
						{
					"LongText", new Dictionary<string, Dictionary<string, object>>[0]
				}
			};
		}

		private static Dictionary<string, object> PrepareSummaryForExtractedTextWithEnabledDataGridInDestination()
							{
			return new Dictionary<string, object>()
								{
									{
					"FieldMapping", new Dictionary<string, int>()
					{
						{
							"LongText", 1
						}
					}
				},
				{
					"ExtractedText", new Dictionary<string, object>()
					{
						{
										"Source", new Dictionary<string, object>()
										{
								{"ArtifactId", 1},
											{"DataGridEnabled", false}
										}
									},
									{
										"Destination", new Dictionary<string, object>()
										{
								{"ArtifactId", 2},
											{"DataGridEnabled", true}
										}
									}
					}
								},
								{
					"LongText", new Dictionary<string, Dictionary<string, object>>[0]
				}
			};
		}

		private static Dictionary<string, object> PrepareSummaryForExtractedTextWithEnabledDataGridInSource()
									{
			return new Dictionary<string, object>()
			{
				{
					"FieldMapping", new Dictionary<string, int>()
					{
						{
							"LongText", 1
						}
					}
				},
				{
					"ExtractedText", new Dictionary<string, object>()
					{
						{
										"Source", new Dictionary<string, object>()
										{
								{"ArtifactId", 1},
											{"DataGridEnabled", true}
										}
									},
									{
										"Destination", new Dictionary<string, object>()
										{
								{"ArtifactId", 2},
											{"DataGridEnabled", false}
										}
									}
								}
				},
				{
					"LongText", new Dictionary<string, Dictionary<string, object>>[0]
							}
			};
						}

		private static Dictionary<string, object> PrepareSummaryForExtractedTextWithDisabledDataGrid()
		{
			return new Dictionary<string, object>()
			{
				{
					"FieldMapping", new Dictionary<string, int>()
					{
						{
							"LongText", 1
					}
				)
				{ TestName = "{m}(ShouldLogSpecialFieldsWhenTheyHaveBeenMapped)" };
		}
				},
				{
					"ExtractedText", new Dictionary<string, object>()
					{
						{
							"Source", new Dictionary<string, object>()
							{
								{"ArtifactId", 1},
								{"DataGridEnabled", false}
							}
						},
						{
							"Destination", new Dictionary<string, object>()
							{
								{"ArtifactId", 2},
								{"DataGridEnabled", false}
							}
						}
					}
				},
				{
					"LongText", new Dictionary<string, Dictionary<string, object>>[0]
				}
			};
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