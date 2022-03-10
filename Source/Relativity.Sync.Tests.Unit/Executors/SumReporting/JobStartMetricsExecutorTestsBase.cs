using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors.SumReporting
{
    internal abstract class JobStartMetricsExecutorTestsBase
    {
        protected Mock<ISyncLog> SyncLogMock;
        protected Mock<ISyncMetrics> SyncMetricsMock;

        protected Mock<IFieldManager> FieldManagerFake;
        protected Mock<IObjectManager> ObjectManagerFake;

        protected Mock<ISourceServiceFactoryForUser> ServiceFactory;

        protected const int SOURCE_WORKSPACE_ARTIFACT_ID = 1;
        protected const int DESTINATION_WORKSPACE_ARTIFACT_ID = 2;

        [SetUp]
        public virtual void SetUp()
        {
            SyncLogMock = new Mock<ISyncLog>();

            SyncMetricsMock = new Mock<ISyncMetrics>();

            FieldManagerFake = new Mock<IFieldManager>();

            ObjectManagerFake = new Mock<IObjectManager>(MockBehavior.Strict);
            ObjectManagerFake.Setup(x => x.Dispose());

            ServiceFactory = new Mock<ISourceServiceFactoryForUser>();
            ServiceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>())
                .ReturnsAsync(ObjectManagerFake.Object);
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

        protected static Dictionary<string, object> PrepareSummaryForLoggingSpecialFields()
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

        protected static Dictionary<string, object> PrepareSummaryForLongText()
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

        protected static Dictionary<string, object> PrepareSummaryForCountingTypes()
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

        protected static Dictionary<string, object> PrepareSummaryForExtractedTextWithDataGridEnabledBothInSourceAndDestination()
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

        protected static Dictionary<string, object> PrepareSummaryForExtractedTextWithEnabledDataGridInDestination()
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

        protected static Dictionary<string, object> PrepareSummaryForExtractedTextWithEnabledDataGridInSource()
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

        protected static Dictionary<string, object> PrepareSummaryForExtractedTextWithDisabledDataGrid()
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

		protected void SetupFieldMapping(IEnumerable<FieldMapDefinitionCase> mapping)
		{
			int artifactIdCounter = 1;
			ObjectManagerFake
				.Setup(x => x.QuerySlimAsync(SOURCE_WORKSPACE_ARTIFACT_ID, It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
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

			ObjectManagerFake
				.Setup(x => x.QuerySlimAsync(DESTINATION_WORKSPACE_ARTIFACT_ID, It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
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


			FieldManagerFake.Setup(x => x.GetMappedFieldsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mapping.Select(x =>
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
