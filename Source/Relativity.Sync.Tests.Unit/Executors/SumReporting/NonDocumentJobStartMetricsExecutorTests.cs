using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.SumReporting;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors.SumReporting
{
	[TestFixture]
	internal class NonDocumentJobStartMetricsExecutorTests
	{
		private Mock<ISyncLog> _syncLogMock;
		private Mock<ISyncMetrics> _syncMetricsMock;
        private Mock<IObjectTypeManager> _objectTypeManagerMock;

		private Mock<IFieldManager> _fieldManagerFake;
		private Mock<IObjectManager> _objectManagerFake;
		private Mock<INonDocumentJobStartMetricsConfiguration> _configurationFake;

        private NonDocumentJobStartMetricsExecutor _sut;

        private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1;
		private const int _DESTINATION_WORKSPACE_ARTIFACT_ID = 2;
		private const int _NON_DOCUMENT_OBJECT_TYPE_ID = (int)ArtifactType.Sync;

		[SetUp]
		public void SetUp()
		{
			_syncLogMock = new Mock<ISyncLog>();
            _syncMetricsMock = new Mock<ISyncMetrics>();
            _objectTypeManagerMock = new Mock<IObjectTypeManager>();

            _fieldManagerFake = new Mock<IFieldManager>();

			_objectManagerFake = new Mock<IObjectManager>(MockBehavior.Strict);
			_objectManagerFake.Setup(x => x.Dispose());

            Mock<ISourceServiceFactoryForUser> serviceFactory = new Mock<ISourceServiceFactoryForUser>();
			serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>())
				.ReturnsAsync(_objectManagerFake.Object);
            serviceFactory.Setup(x => x.CreateProxyAsync<IObjectTypeManager>())
                .ReturnsAsync(_objectTypeManagerMock.Object);

			_configurationFake = new Mock<INonDocumentJobStartMetricsConfiguration>();
			_configurationFake.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ARTIFACT_ID);
			_configurationFake.SetupGet(x => x.DestinationWorkspaceArtifactId).Returns(_DESTINATION_WORKSPACE_ARTIFACT_ID);
			_configurationFake.SetupGet(x => x.RdoArtifactTypeId).Returns(_NON_DOCUMENT_OBJECT_TYPE_ID);
            
            PrepareTestData();

			_sut = new NonDocumentJobStartMetricsExecutor(
                serviceFactory.Object,
				_syncLogMock.Object,
				_syncMetricsMock.Object,
				_fieldManagerFake.Object);
        }

		[Test]
		public async Task ExecuteAsync_ShouldReportJobStartMetric()
		{
            // Act
			await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			_syncMetricsMock.Verify(x => x.Send(It.Is<NonDocumentJobStartMetric>(m => 
				m.Type == TelemetryConstants.PROVIDER_NAME &&
				m.FlowType == TelemetryConstants.FLOW_TYPE_SAVED_SEARCH_NON_DOCUMENT_OBJECTS)));
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
		public async Task ExecuteAsync_ShouldLogFieldsMappingDetails()
		{
			// Arrange
			_fieldManagerFake.Setup(x => x.GetMappedFieldsAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(new List<FieldInfoDto>());

			// Act
			await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			_syncLogMock.Verify(x => x.LogInformation("Fields map configuration summary: {@summary}", It.IsAny<Dictionary<string, object>>()));
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
			_fieldManagerFake.Setup(x => x.GetMappedFieldsAsync(It.IsAny<CancellationToken>()))
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
			_syncLogMock.Verify(x => x.LogInformation("Fields map configuration summary: {@summary}", It.Is<Dictionary<string, object>>(actual => verify(actual))));
		}

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

			_syncLogMock.Verify(x => x.LogInformation("Fields map configuration summary: {@summary}", It.IsAny<Dictionary<string, object>>()), Times.Never);
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
			{ TestName = "{m}(ExtractedTextDataGridSource=disable, ExtractedTextDataGridDestination=disabled)" };

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
						}
					},
					PrepareSummaryForLongText()
				)
			{ TestName = "{m}(LongText)" };

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
					PrepareSummaryForLoggingSpecialFields()
				)
			{ TestName = "{m}(ShouldLogSpecialFieldsWhenTheyHaveBeenMapped)" };
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
			_objectTypeManagerMock.Setup(x => x.ReadAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _NON_DOCUMENT_OBJECT_TYPE_ID))
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

			_fieldManagerFake.Setup(x => x.GetMappedFieldsAsync(It.IsAny<CancellationToken>()))
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

			_objectManagerFake.Setup(x => x.QuerySlimAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(),
					fieldsDtos.Count, It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(queryResultSlim));
		}

		private static Dictionary<string, object> PrepareSummaryForLoggingSpecialFields()
		{
			return new Dictionary<string, object>()
			{
				{
					"FieldMapping", new Dictionary<string, int>()
					{
						{
							"LongText", 5
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
						},
						new Dictionary<string, Dictionary<string, object>>()
						{
							{
								"Source", new Dictionary<string, object>()
								{
									{"ArtifactId", 4},
									{"DataGridEnabled", true}
								}
							},
							{
								"Destination", new Dictionary<string, object>()
								{
									{"ArtifactId", 9},
									{"DataGridEnabled", false}
								}
							}
						},
						new Dictionary<string, Dictionary<string, object>>()
						{
							{
								"Source", new Dictionary<string, object>()
								{
									{"ArtifactId", 5},
									{"DataGridEnabled", true}
								}
							},
							{
								"Destination", new Dictionary<string, object>()
								{
									{"ArtifactId", 10},
									{"DataGridEnabled", false}
								}
							}
						},
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
			_objectManagerFake
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

			_objectManagerFake
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


			_fieldManagerFake.Setup(x => x.GetMappedFieldsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mapping.Select(x =>
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
