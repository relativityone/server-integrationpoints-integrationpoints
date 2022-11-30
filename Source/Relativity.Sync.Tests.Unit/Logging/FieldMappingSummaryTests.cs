using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Logging
{
    [TestFixture]
    public class FieldMappingSummaryTests
    {
        internal class FieldMapDefinitionCase
        {
            public string SourceFieldName { get; set; }

            public string DestinationFieldName { get; set; }

            public RelativityDataType DataType { get; set; }

            public bool SourceFieldDataGridEnabled { get; set; }

            public bool DestinationFieldDataGridEnabled { get; set; }

            public SpecialFieldType SpecialFieldType { get; set; } = SpecialFieldType.None;
        }

        private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1;
        private const int _DESTINATION_WORKSPACE_ARTIFACT_ID = 2;

        private Mock<IFieldConfiguration> _configurationFake;
        private Mock<IFieldManager> _fieldManagerFake;
        private Mock<ISourceServiceFactoryForUser> _serviceFactory;
        private Mock<IObjectManager> _objectManager;

        private FieldMappingSummary _sut;

        [SetUp]
        public void SetUp()
        {
            _configurationFake = new Mock<IFieldConfiguration>();
            _configurationFake.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ARTIFACT_ID);
            _configurationFake.SetupGet(x => x.DestinationWorkspaceArtifactId).Returns(_DESTINATION_WORKSPACE_ARTIFACT_ID);

            _fieldManagerFake = new Mock<IFieldManager>();
            _serviceFactory = new Mock<ISourceServiceFactoryForUser>();
            _objectManager = new Mock<IObjectManager>();
            _serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
            _sut = new FieldMappingSummary(_configurationFake.Object, _fieldManagerFake.Object, _serviceFactory.Object, new EmptyLogger());
        }

        [TestCaseSource(nameof(FieldsMappingTestCaseSource))]
        public async Task ExecuteAsync_ShouldLogFieldsMappingDetails(IEnumerable<object> mapping, Dictionary<string, object> expected)
        {
            // Arrange
            PrepareTestData();
            SetupFieldMapping(mapping.Cast<FieldMapDefinitionCase>());

            // Act
            Dictionary<string, object> actual = await _sut.GetFieldsMappingSummaryAsync(CancellationToken.None).ConfigureAwait(false);

            // Assert
            CollectionAssert.AreEquivalent(expected, actual);
        }

        private void SetupFieldMapping(IEnumerable<FieldMapDefinitionCase> mapping)
        {
            int artifactIdCounter = 1;
            _objectManager
                .Setup(x => x.QuerySlimAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new QueryResultSlim
                {
                    Objects = mapping.Select(x =>
                        new RelativityObjectSlim
                        {
                            ArtifactID = artifactIdCounter++,
                            Values = new List<object> { x.SourceFieldName, x.SourceFieldDataGridEnabled }
                        })
                    .ToList()
                });

            _objectManager
                .Setup(x => x.QuerySlimAsync(_DESTINATION_WORKSPACE_ARTIFACT_ID, It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new QueryResultSlim
                {
                    Objects = mapping.Select(x =>
                        new RelativityObjectSlim
                        {
                            ArtifactID = artifactIdCounter++,
                            Values = new List<object> { x.DestinationFieldName, x.DestinationFieldDataGridEnabled }
                        })
                    .ToList()
                });

            _fieldManagerFake.Setup(x => x.GetMappedFieldsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mapping.Select(x =>
                new FieldInfoDto(x.SpecialFieldType, x.SourceFieldName, x.DestinationFieldName, true, true) { RelativityDataType = x.DataType })
            .ToList);
        }

        private void PrepareTestData()
        {
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

            _objectManager.Setup(x => x.QuerySlimAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(),
                    fieldsDtos.Count, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(queryResultSlim));
        }

        private static IEnumerable<TestCaseData> FieldsMappingTestCaseSource()
        {
            const string fieldName = "Field name";

            yield return new TestCaseData(
                    new List<FieldMapDefinitionCase>
                    {
                        new FieldMapDefinitionCase
                        {
                            SourceFieldName = fieldName, DestinationFieldName = fieldName,
                            DataType = RelativityDataType.LongText
                        },
                    }, PrepareSummaryWithDisabledDataGrid())
            { TestName = "{m}(DataGridSource=disable, DataGridDestination=disabled)" };

            yield return new TestCaseData(
                    new List<FieldMapDefinitionCase>
                    {
                        new FieldMapDefinitionCase
                        {
                            SourceFieldName = fieldName, DestinationFieldName = fieldName,
                            DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = true,
                            DestinationFieldDataGridEnabled = false
                        }
                    }, PrepareSummaryWithEnabledDataGridInSource())
            { TestName = "{m}(DataGridSource=enabled, DataGridDestination=disabled)" };

            yield return new TestCaseData(
                    new List<FieldMapDefinitionCase>
                    {
                        new FieldMapDefinitionCase
                        {
                            SourceFieldName = fieldName, DestinationFieldName = fieldName,
                            DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = false,
                            DestinationFieldDataGridEnabled = true
                        }
                    }, PrepareSummaryWithEnabledDataGridInDestination())
            { TestName = "{m}(DataGridSource=disabled, DataGridDestination=enabled)" };

            yield return new TestCaseData(
                    new List<FieldMapDefinitionCase>
                    {
                        new FieldMapDefinitionCase
                        {
                            SourceFieldName = fieldName, DestinationFieldName = fieldName,
                            DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = true,
                            DestinationFieldDataGridEnabled = true
                        }
                    }, PrepareSummaryWithDataGridEnabledBothInSourceAndDestination())
            { TestName = "{m}(DataGridSource=enabled, tDataGridDestination=enabled)" };

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
                            SourceFieldName = "randomName", DestinationFieldName = "CommandoreBombardiero",
                            DataType = RelativityDataType.FixedLengthText
                        },
                        new FieldMapDefinitionCase
                        {
                            SourceFieldName = "AdlerSieben", DestinationFieldName = "SeniorGordo",
                            DataType = RelativityDataType.FixedLengthText
                        },
                        new FieldMapDefinitionCase
                            { SourceFieldName = "1", DestinationFieldName = "1", DataType = RelativityDataType.Currency },
                        new FieldMapDefinitionCase
                            { SourceFieldName = "2", DestinationFieldName = "2", DataType = RelativityDataType.Currency },
                        new FieldMapDefinitionCase
                            { SourceFieldName = "3", DestinationFieldName = "3", DataType = RelativityDataType.YesNo },
                        new FieldMapDefinitionCase
                            { SourceFieldName = "4", DestinationFieldName = "4", DataType = RelativityDataType.YesNo },
                        new FieldMapDefinitionCase
                            { SourceFieldName = "5", DestinationFieldName = "5", DataType = RelativityDataType.YesNo },
                        new FieldMapDefinitionCase
                            { SourceFieldName = "6", DestinationFieldName = "6", DataType = RelativityDataType.YesNo },
                    }, PrepareSummaryForCountingTypes())
            { TestName = "{m}(CountingTypes)" };

            yield return new TestCaseData(
                    new List<FieldMapDefinitionCase>
                    {
                        new FieldMapDefinitionCase
                        {
                            SourceFieldName = fieldName, DestinationFieldName = fieldName,
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
                    }, PrepareSummaryForLongText())
            { TestName = "{m}(LongText)" };

            yield return new TestCaseData(
                    new List<FieldMapDefinitionCase>
                    {
                        new FieldMapDefinitionCase
                        {
                            SourceFieldName = fieldName, DestinationFieldName = fieldName,
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
                            SourceFieldName = "supported by viewer 1", DestinationFieldName = "supported by viewer 1",
                            DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = true,
                            DestinationFieldDataGridEnabled = false,
                            SpecialFieldType = SpecialFieldType.SupportedByViewer
                        },
                        new FieldMapDefinitionCase
                        {
                            SourceFieldName = "supported by viewer 2", DestinationFieldName = "supported by viewer 2",
                            DataType = RelativityDataType.LongText, SourceFieldDataGridEnabled = true,
                            DestinationFieldDataGridEnabled = false,
                            SpecialFieldType = SpecialFieldType.SupportedByViewer
                        }
                    }, PrepareSummaryForLoggingSpecialFields())
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
                    "LongText", new Dictionary<string, Dictionary<string, object>>[]
                    {
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "Source", new Dictionary<string, object>()
                                {
                                    { "ArtifactId", 1 },
                                    { "DataGridEnabled", true }
                                }
                            },
                            {
                                "Destination", new Dictionary<string, object>()
                                {
                                    { "ArtifactId", 6 },
                                    { "DataGridEnabled", true }
                                }
                            }
                        },
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "Source", new Dictionary<string, object>()
                                {
                                    { "ArtifactId", 2 },
                                    { "DataGridEnabled", false }
                                }
                            },
                            {
                                "Destination", new Dictionary<string, object>()
                                {
                                    { "ArtifactId", 7 },
                                    { "DataGridEnabled", true }
                                }
                            }
                        },
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "Source", new Dictionary<string, object>()
                                {
                                    { "ArtifactId", 3 },
                                    { "DataGridEnabled", true }
                                }
                            },
                            {
                                "Destination", new Dictionary<string, object>()
                                {
                                    { "ArtifactId", 8 },
                                    { "DataGridEnabled", false }
                                }
                            }
                        },
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "Source", new Dictionary<string, object>()
                                {
                                    { "ArtifactId", 4 },
                                    { "DataGridEnabled", true }
                                }
                            },
                            {
                                "Destination", new Dictionary<string, object>()
                                {
                                    { "ArtifactId", 9 },
                                    { "DataGridEnabled", false }
                                }
                            }
                        },
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "Source", new Dictionary<string, object>()
                                {
                                    { "ArtifactId", 5 },
                                    { "DataGridEnabled", true }
                                }
                            },
                            {
                                "Destination", new Dictionary<string, object>()
                                {
                                    { "ArtifactId", 10 },
                                    { "DataGridEnabled", false }
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
                    "LongText", new Dictionary<string, Dictionary<string, object>>[]
                    {
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "Source", new Dictionary<string, object>()
                                {
                                    { "ArtifactId", 1 },
                                    { "DataGridEnabled", true }
                                }
                            },
                            {
                                "Destination", new Dictionary<string, object>()
                                {
                                    { "ArtifactId", 4 },
                                    { "DataGridEnabled", true }
                                }
                            }
                        },
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "Source", new Dictionary<string, object>()
                                {
                                    { "ArtifactId", 2 },
                                    { "DataGridEnabled", false }
                                }
                            },
                            {
                                "Destination", new Dictionary<string, object>()
                                {
                                    { "ArtifactId", 5 },
                                    { "DataGridEnabled", true }
                                }
                            }
                        },
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "Source", new Dictionary<string, object>()
                                {
                                    { "ArtifactId", 3 },
                                    { "DataGridEnabled", true }
                                }
                            },
                            {
                                "Destination", new Dictionary<string, object>()
                                {
                                    { "ArtifactId", 6 },
                                    { "DataGridEnabled", false }
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
                    "LongText", new Dictionary<string, Dictionary<string, object>>[0]
                }
            };
        }

        private static Dictionary<string, object> PrepareSummaryWithDataGridEnabledBothInSourceAndDestination()
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
                    "LongText", new Dictionary<string, Dictionary<string, object>>[]
                    {
                        new Dictionary<string, Dictionary<string, object>>
                        {
                            {
                                "Source", new Dictionary<string, object>
                                {
                                    { "ArtifactId", 1 },
                                    { "DataGridEnabled", true }
                                }
                            },
                            {
                                "Destination", new Dictionary<string, object>
                                {
                                    { "ArtifactId", 2 },
                                    { "DataGridEnabled", true }
                                }
                            }
                        }
                    }
                }
            };
        }

        private static Dictionary<string, object> PrepareSummaryWithEnabledDataGridInDestination()
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
                    "LongText", new Dictionary<string, Dictionary<string, object>>[]
                    {
                        new Dictionary<string, Dictionary<string, object>>
                        {
                            {
                                "Source", new Dictionary<string, object>
                                {
                                    { "ArtifactId", 1 },
                                    { "DataGridEnabled", false }
                                }
                            },
                            {
                                "Destination", new Dictionary<string, object>
                                {
                                    { "ArtifactId", 2 },
                                    { "DataGridEnabled", true }
                                }
                            }
                        }
                    }
                }
            };
        }

        private static Dictionary<string, object> PrepareSummaryWithEnabledDataGridInSource()
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
                    "LongText", new Dictionary<string, Dictionary<string, object>>[]
                    {
                        new Dictionary<string, Dictionary<string, object>>
                        {
                            {
                                "Source", new Dictionary<string, object>
                                {
                                    { "ArtifactId", 1 },
                                    { "DataGridEnabled", true }
                                }
                            },
                            {
                                "Destination", new Dictionary<string, object>
                                {
                                    { "ArtifactId", 2 },
                                    { "DataGridEnabled", false }
                                }
                            }
                        }
                    }
                }
            };
        }

        private static Dictionary<string, object> PrepareSummaryWithDisabledDataGrid()
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
                    "LongText", new Dictionary<string, Dictionary<string, object>>[]
                    {
                        new Dictionary<string, Dictionary<string, object>>
                        {
                            {
                                "Source", new Dictionary<string, object>
                                {
                                    { "ArtifactId", 1 },
                                    { "DataGridEnabled", false }
                                }
                            },
                            {
                                "Destination", new Dictionary<string, object>
                                {
                                    { "ArtifactId", 2 },
                                    { "DataGridEnabled", false }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
