﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.SumReporting;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors.SumReporting
{
	[TestFixture]
	internal class DocumentJobStartMetricsExecutorTests : JobStartMetricsExecutorTestsBase
	{
		private Mock<IFileStatisticsCalculator> _fileStatisticsCalculatorFake;
		private Mock<IDocumentJobStartMetricsConfiguration> _configurationFake;

		private IJobStatisticsContainer _jobStatisticsContainer;

		private DocumentJobStartMetricsExecutor _sut;

		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1;
		private const int _DESTINATION_WORKSPACE_ARTIFACT_ID = 2;

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
            _fileStatisticsCalculatorFake = new Mock<IFileStatisticsCalculator>();

			_jobStatisticsContainer = new JobStatisticsContainer();

			Mock<ISnapshotQueryRequestProvider> queryRequestProvider = new Mock<ISnapshotQueryRequestProvider>();

			_configurationFake = new Mock<IDocumentJobStartMetricsConfiguration>();
			_configurationFake.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ARTIFACT_ID);
			_configurationFake.SetupGet(x => x.DestinationWorkspaceArtifactId).Returns(_DESTINATION_WORKSPACE_ARTIFACT_ID);

			_sut = new DocumentJobStartMetricsExecutor(
				SyncLogMock.Object,
				SyncMetricsMock.Object,
				FieldManagerFake.Object,
				ServiceFactory.Object,
				_jobStatisticsContainer,
				_fileStatisticsCalculatorFake.Object,
				queryRequestProvider.Object);
		}

		[Test]
		public async Task ExecuteAsync_ShouldReportJobStartMetric()
		{
			// Act
			await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			SyncMetricsMock.Verify(x => x.Send(It.Is<JobStartMetric>(m => 
				m.Type == TelemetryConstants.PROVIDER_NAME &&
				m.FlowType == TelemetryConstants.FLOW_TYPE_SAVED_SEARCH_NATIVES_AND_METADATA)));
		}

		[Test]
		public async Task ExecuteAsync_ShouldReportRetryMetric_WhenRetryFlowIsSelected()
		{
			// Arrange
			_configurationFake.SetupGet(x => x.JobHistoryToRetryId).Returns(100);

			// Act
			await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			SyncMetricsMock.Verify(x => x.Send(It.Is<JobStartMetric>(m => m.RetryType != null)));
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task ExecuteAsync_ShouldSetNativesBytesRequestedInStatisticsContainer(bool isResuming)
		{
			// Arrange
			_configurationFake.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.CopyFiles);
			_configurationFake.SetupGet(x => x.Resuming).Returns(isResuming);

			const long expectedNativesBytesRequested = 100;

			_fileStatisticsCalculatorFake.Setup(x =>
					x.CalculateNativesTotalSizeAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.IsAny<QueryRequest>(), It.IsAny<CompositeCancellationToken>()))
				.ReturnsAsync(expectedNativesBytesRequested);

			// Act
			await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			long nativesBytesRequested = await _jobStatisticsContainer.NativesBytesRequested.ConfigureAwait(false);
			nativesBytesRequested.Should().Be(expectedNativesBytesRequested);
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task ExecuteAsync_ShouldSetNativesBytesRequestedToZero_WhenDoNotImportNatives(bool isResuming)
		{
			// Arrange
			_configurationFake.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.DoNotImportNativeFiles);
			_configurationFake.SetupGet(x => x.Resuming).Returns(isResuming);

			const long expectedNativesBytesRequested = 0;

			_fileStatisticsCalculatorFake.Setup(x =>
					x.CalculateNativesTotalSizeAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.IsAny<QueryRequest>(), It.IsAny<CompositeCancellationToken>()))
				.ReturnsAsync(expectedNativesBytesRequested);

			// Act
			await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			long nativesBytesRequested = await _jobStatisticsContainer.NativesBytesRequested.ConfigureAwait(false);
			nativesBytesRequested.Should().Be(expectedNativesBytesRequested);
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

		[Test]
		public async Task ExecuteAsync_ShouldNotCallObjectManager_WhenThereIsNoLongTextFieldsInMapping()
		{
			// Act
			await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);
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
			SyncMetricsMock.Verify(x => x.Send(It.IsAny<JobStartMetric>()), Times.Never);

			SyncLogMock.Verify(x => x.LogInformation("Fields map configuration summary: {@summary}", It.IsAny<Dictionary<string, object>>()), Times.Never);
		}

        private static IEnumerable<TestCaseData> FieldsMappingTestCaseSource()
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
                    }, PrepareSummaryForExtractedTextWithDisabledDataGrid()
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
                    }, PrepareSummaryForExtractedTextWithEnabledDataGridInSource()
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
                    }, PrepareSummaryForExtractedTextWithEnabledDataGridInDestination()
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
                    }, PrepareSummaryForExtractedTextWithDataGridEnabledBothInSourceAndDestination()
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
                    }, PrepareSummaryForCountingTypes()
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
                    }, PrepareSummaryForLongText()
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
                    }, PrepareSummaryForLoggingSpecialFields()
                )
                { TestName = "{m}(ShouldLogSpecialFieldsWhenTheyHaveBeenMapped)" };
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
    }
}
