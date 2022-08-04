using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Domain.Models;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Metrics;
using Relativity.Services.Field;
using Relativity.Services.Search;
using Relativity.Services.View;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace Relativity.IntegrationPoints.FieldsMapping.Tests
{
    [TestFixture, Category("Unit")]
    public class AutomapRunnerTests
    {
        private const string DestinationProviderGuid = "6C486C1B-9AEA-4809-B4F8-7123A27A0D6E";
        private const int WorkspaceArtifactId = 1234;

        private Mock<IKeywordSearchManager> _keywordSearchManagerFake;
        private Mock<IViewManager> _viewManagerFake;
        private Mock<IServicesMgr> _servicesMgrFake;
        private Mock<IMetricBucketNameGenerator> _metricBucketNameGeneratorFake;
        private Mock<IMetricsSender> _metricsSenderMock;
        private AutomapRunner _sut;

        [SetUp]
        public void Setup()
        {
            _servicesMgrFake = new Mock<IServicesMgr>();

            _keywordSearchManagerFake = new Mock<IKeywordSearchManager>();
            _servicesMgrFake.Setup(x => x.CreateProxy<IKeywordSearchManager>(It.IsAny<ExecutionIdentity>()))
                .Returns(_keywordSearchManagerFake.Object);

            _viewManagerFake = new Mock<IViewManager>();
            _servicesMgrFake.Setup(x => x.CreateProxy<IViewManager>(It.IsAny<ExecutionIdentity>()))
                .Returns(_viewManagerFake.Object);

            _metricBucketNameGeneratorFake = new Mock<IMetricBucketNameGenerator>();

            _metricsSenderMock = new Mock<IMetricsSender>();
            _metricsSenderMock.Setup(x => x.GaugeOperation(
                It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()));

            _sut = new AutomapRunner(_servicesMgrFake.Object, _metricsSenderMock.Object, _metricBucketNameGeneratorFake.Object);
        }

        [Test]
        public void MapFields_ShouldMapFieldsWithTheSameTypeAndName()
        {
            // Arrange
            var sourceFields = new[]
            {
                new FieldInfo(fieldIdentifier: "1", name: "Field 1", type: "Type 1"),
                new FieldInfo(fieldIdentifier: "2", name: "Field 2", type: "Type 2")
            };

            var destinationFields = new[]
            {
                new FieldInfo(fieldIdentifier: "3", name: "Field 1", type: "Type 1"),
                new FieldInfo(fieldIdentifier: "4", name: "Field 2", type: "Type 2")
            };

            // Act
            var mappedFields = _sut.MapFields(sourceFields, destinationFields, DestinationProviderGuid, WorkspaceArtifactId).ToArray();

            // Assert
            mappedFields.Count().Should().Be(2);

            mappedFields[0].SourceField.DisplayName.Should().Be(mappedFields[0].DestinationField.DisplayName);
            mappedFields[0].SourceField.Type.Should().Be(mappedFields[0].DestinationField.Type);
            mappedFields[0].FieldMapType.Should().Be(FieldMapTypeEnum.None);

            mappedFields[1].SourceField.DisplayName.Should().Be(mappedFields[1].DestinationField.DisplayName);
            mappedFields[1].SourceField.Type.Should().Be(mappedFields[1].DestinationField.Type);
            mappedFields[1].FieldMapType.Should().Be(FieldMapTypeEnum.None);
        }

        [Test]
        public void MapFields_ShouldNotMapFieldsWithDifferentNameButTheSameIdentifier()
        {
            // Arrange
            var sourceFields = new[]
            {
                new FieldInfo(fieldIdentifier: "1", name: "Field 1", type: "Type 1"),
                new FieldInfo(fieldIdentifier: "2", name: "Field 2", type: "Type 2")
            };

            var destinationFields = new[]
            {
                new FieldInfo(fieldIdentifier: "1", name: "Field 3", type: "Type 1"),
                new FieldInfo(fieldIdentifier: "2", name: "Field 4", type: "Type 2")
            };

            // Act
            var mappedFields = _sut.MapFields(sourceFields, destinationFields, DestinationProviderGuid, WorkspaceArtifactId).ToArray();

            // Assert
            mappedFields.Should().BeEmpty();
        }

        [Test]
        public void MapFields_ShouldNotMapFieldsWithTheSameNameButDifferentType()
        {
            // Arrange
            var sourceFields = new[]
            {
                new FieldInfo(fieldIdentifier: "1", name: "Field 1", type: "Type 1"),
                new FieldInfo(fieldIdentifier: "2", name: "Field 2", type: "Type 2")
            };

            var destinationFields = new[]
            {
                new FieldInfo(fieldIdentifier: "3", name: "Field 3", type: "Type 1"),
                new FieldInfo(fieldIdentifier: "4", name: "Field 2", type: "Type 3")
            };

            // Act
            var mappedFields = _sut.MapFields(sourceFields, destinationFields, DestinationProviderGuid, WorkspaceArtifactId).ToArray();

            // Assert
            mappedFields.Count().Should().Be(0);
        }

        [Test]
        public void MapFields_ShouldMapFixedLengthTextToLongText()
        {
            // Arrange
            var sourceFields = new[]
            {
                new FieldInfo(fieldIdentifier: "1", name: "Field 1", type: "Fixed-Length Text(250)")
            };

            var destinationFields = new[]
            {
                new FieldInfo(fieldIdentifier: "2", name: "Field 1", type: "Long Text")
            };

            // Act
            var mappedFields = _sut.MapFields(sourceFields, destinationFields, DestinationProviderGuid, WorkspaceArtifactId).ToArray();

            // Assert
            mappedFields.Count().Should().Be(1);

            mappedFields[0].SourceField.DisplayName.Should().Be(mappedFields[0].DestinationField.DisplayName);
            mappedFields[0].SourceField.Type.Should().Be("Fixed-Length Text(250)");
            mappedFields[0].DestinationField.Type.Should().Be("Long Text");
        }


        [Test]
        public void MapFields_ShouldNotMapFixedLengthTextToFixedLentghText_WhenSourceIsGreaterThanDestination()
        {
            // Arrange
            var sourceFields = new[]
            {
                new FieldInfo(fieldIdentifier: "1", name: "Field 1", type: "Fixed-Length Text(250)")
            };

            var destinationFields = new[]
            {
                new FieldInfo(fieldIdentifier: "2", name: "Field 1", type: "Fixed-Length Text(50)")
            };

            // Act
            var mappedFields = _sut.MapFields(sourceFields, destinationFields, DestinationProviderGuid, WorkspaceArtifactId).ToArray();

            // Assert
            mappedFields.Count().Should().Be(0);
        }

        [Test]
        public void MapFields_ShouldMapFixedLengthTextToFixedLentghText_WhenSourceIsLowerThanDestination()
        {
            // Arrange
            var sourceFields = new[]
            {
                new FieldInfo(fieldIdentifier: "1", name: "Field 1", type: "Fixed-Length Text(50)")
            };

            var destinationFields = new[]
            {
                new FieldInfo(fieldIdentifier: "2", name: "Field 1", type: "Fixed-Length Text(250)")
            };

            // Act
            var mappedFields = _sut.MapFields(sourceFields, destinationFields, DestinationProviderGuid, WorkspaceArtifactId).ToArray();

            // Assert
            mappedFields.Count().Should().Be(1);

            mappedFields[0].SourceField.DisplayName.Should().Be(mappedFields[0].DestinationField.DisplayName);
            mappedFields[0].SourceField.Type.Should().Be("Fixed-Length Text(50)");
            mappedFields[0].DestinationField.Type.Should().Be("Fixed-Length Text(250)");
        }

        [Test]
        public void MapFields_ShouldMapIdentifiersWithDifferentNames()
        {
            // Arrange
            var sourceFields = new[]
            {
                new FieldInfo(fieldIdentifier: "1", name: "Field 1", type: "Fixed-Length Text(250)")
                {
                    IsIdentifier = true
                }
            };

            var destinationFields = new[]
            {
                new FieldInfo(fieldIdentifier: "2", name: "Field 1", type: "Fixed-Length Text(50)"),
                new FieldInfo(fieldIdentifier: "3", name: "Field 2", type: "Fixed-Length Text(250)")
                {
                    IsIdentifier = true
                }
            };

            // Act
            var mappedFields = _sut.MapFields(sourceFields, destinationFields, DestinationProviderGuid, WorkspaceArtifactId).ToArray();

            // Assert
            mappedFields.Count().Should().Be(1);
            mappedFields[0].SourceField.DisplayName.Should().Be("Field 1");
            mappedFields[0].DestinationField.DisplayName.Should().Be("Field 2");
            mappedFields[0].FieldMapType.Should().Be(FieldMapTypeEnum.Identifier);
        }

        [Test]
        public void MapFields_ShouldMapIdentifiersWithDifferentLengths()
        {
            // Arrange
            var sourceFields = new[]
            {
                new FieldInfo(fieldIdentifier: "1", name: "Field 1", type: "Fixed-Length Text(250)")
                {
                    IsIdentifier = true
                }
            };

            var destinationFields = new[]
            {
                new FieldInfo(fieldIdentifier: "2", name: "Field 2", type: "Fixed-Length Text(50)")
                {
                    IsIdentifier = true
                }
            };

            // Act
            var mappedFields = _sut.MapFields(sourceFields, destinationFields, DestinationProviderGuid, WorkspaceArtifactId).ToArray();

            // Assert
            mappedFields.Count().Should().Be(1);
            mappedFields[0].SourceField.DisplayName.Should().Be("Field 1");
            mappedFields[0].DestinationField.DisplayName.Should().Be("Field 2");
            mappedFields[0].FieldMapType.Should().Be(FieldMapTypeEnum.Identifier);
        }

        [Test]
        public void MapFields_ShouldMapOnlyIdentifiers_WhenParameterIsSet()
        {
            // Arrange
            var sourceFields = new[]
            {
                new FieldInfo(fieldIdentifier: "1", name: "Field 1", type: "Fixed-Length Text(250)")
                {
                    IsIdentifier = true
                },
                new FieldInfo(fieldIdentifier: "2", name: "Field 2", type: "Fixed-Length Text(250)")
            };

            var destinationFields = new[]
            {
                new FieldInfo(fieldIdentifier: "3", name: "Field 1", type: "Fixed-Length Text(250)")
                {
                    IsIdentifier = true
                },
                new FieldInfo(fieldIdentifier: "4", name: "Field 2", type: "Fixed-Length Text(50)")
            };

            // Act
            var mappedFields = _sut.MapFields(sourceFields, destinationFields, DestinationProviderGuid, WorkspaceArtifactId, true).ToArray();

            // Assert
            mappedFields.Count().Should().Be(1);
            mappedFields[0].SourceField.DisplayName.Should().Be("Field 1");
            mappedFields[0].DestinationField.DisplayName.Should().Be("Field 1");
            mappedFields[0].FieldMapType.Should().Be(FieldMapTypeEnum.Identifier);
        }

        [Test]
        public void MapFields_ShouldReturnMappingsInAlphabeticalOrder()
        {
            // Arrange
            var sourceFields = new[]
            {
                new FieldInfo(fieldIdentifier: "1", name: "Field 1", type: "Fixed-Length Text(250)"),
                new FieldInfo(fieldIdentifier: "2", name: "Field 3", type: "Fixed-Length Text(250)"),
                new FieldInfo(fieldIdentifier: "3", name: "Field 2", type: "Fixed-Length Text(250)")
            };

            var destinationFields = new[]
            {
                new FieldInfo(fieldIdentifier: "4", name: "Field 3", type: "Fixed-Length Text(250)"),
                new FieldInfo(fieldIdentifier: "5", name: "Field 2", type: "Fixed-Length Text(250)"),
                new FieldInfo(fieldIdentifier: "6", name: "Field 1", type: "Fixed-Length Text(250)")
            };

            // Act
            var mappedFields = _sut.MapFields(sourceFields, destinationFields, DestinationProviderGuid, WorkspaceArtifactId).ToArray();

            // Assert
            mappedFields.Count().Should().Be(3);
            mappedFields[0].SourceField.DisplayName.Should().Be("Field 1");
            mappedFields[1].SourceField.DisplayName.Should().Be("Field 2");
            mappedFields[2].SourceField.DisplayName.Should().Be("Field 3");
        }

        [Test]
        public async Task MapFieldsFromSavedSearch_ShouldMapFieldFromSavedSearch()
        {
            // Arrange

            var savedSearchFields = new List<FieldRef>
            {
                new FieldRef()
                {
                    ArtifactID = 2,
                    Name = "Field 2"
                }
            };

            var sourceFields = new[]
            {
                new FieldInfo("1", "Field 1", "Fixed-Length Text(250)"),
                new FieldInfo("2", "Field 2", "Fixed-Length Text(250)")
            };

            var destinationFields = new[]
            {
                new FieldInfo("3", "Field 3", "Fixed-Length Text(250)"),
                new FieldInfo("4", "Field 2", "Fixed-Length Text(250)")
            };

            _keywordSearchManagerFake.Setup(x => x.ReadSingleAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new KeywordSearch()
                {
                    Fields = savedSearchFields
                });

            // Act
            var mappedFields = (await _sut.MapFieldsFromSavedSearchAsync(sourceFields, destinationFields, DestinationProviderGuid, 1, 2)
                .ConfigureAwait(false)).ToArray();

            // Assert
            mappedFields.Length.Should().Be(1);
            mappedFields.Single().SourceField.DisplayName.Should().Be("Field 2");
        }

        [Test]
        public async Task MapFieldsFromSavedSearch_ShouldMapAlsoObjectIdentifier()
        {
            // Arrange

            var savedSearchFields = new List<FieldRef>
            {
                new FieldRef()
                {
                    ArtifactID = 2,
                    Name = "Field 2"
                }
            };

            var sourceFields = new[]
            {
                new FieldInfo("1", "Control Number", "Fixed-Length Text(250)")
                {
                    IsIdentifier = true
                },
                new FieldInfo("2", "Field 2", "Fixed-Length Text(250)")
            };

            var destinationFields = new[]
            {
                new FieldInfo("10", "Control Number", "Fixed-Length Text(250)")
                {
                    IsIdentifier = true
                },
                new FieldInfo("3", "Field 3", "Fixed-Length Text(250)"),
                new FieldInfo("4", "Field 2", "Fixed-Length Text(250)"),
                new FieldInfo("5", "Field 5", "Fixed-Length Text(250)")
            };

            _keywordSearchManagerFake.Setup(x => x.ReadSingleAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new KeywordSearch()
                {
                    Fields = savedSearchFields
                });

            // Act
            var mappedFields = (await _sut.MapFieldsFromSavedSearchAsync(sourceFields, destinationFields, DestinationProviderGuid, 1, 2)
                .ConfigureAwait(false)).ToArray();

            // Assert
            mappedFields.Length.Should().Be(2);
            mappedFields[0].SourceField.DisplayName.Should().Be("Control Number");
            mappedFields[1].SourceField.DisplayName.Should().Be("Field 2");
        }

        [Test]
        public async Task MapFieldsFromView_ShouldMapFieldFromView()
        {
            // Arrange

            var viewFields = new List<FieldRef>
            {
                new FieldRef()
                {
                    ArtifactID = 2,
                    Name = "Field 2"
                }
            };

            var sourceFields = new[]
            {
                new FieldInfo("1", "Field 1", "Fixed-Length Text(250)"),
                new FieldInfo("2", "Field 2", "Fixed-Length Text(250)")
            };

            var destinationFields = new[]
            {
                new FieldInfo("3", "Field 3", "Fixed-Length Text(250)"),
                new FieldInfo("4", "Field 2", "Fixed-Length Text(250)")
            };

            _viewManagerFake.Setup(x => x.ReadSingleAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new View()
                {
                    Fields = viewFields
                });

            // Act
            Models.FieldMap[] mappedFields = (await _sut.MapFieldsFromViewAsync(sourceFields, destinationFields, DestinationProviderGuid, 1, 2)
                .ConfigureAwait(false)).ToArray();

            // Assert
            mappedFields.Length.Should().Be(1);
            mappedFields.Single().SourceField.DisplayName.Should().Be("Field 2");
        }

        [Test]
        public async Task MapFieldsFromView_ShouldMapAlsoObjectIdentifier()
        {
            // Arrange

            var viewFields = new List<FieldRef>
            {
                new FieldRef()
                {
                    ArtifactID = 2,
                    Name = "Field 2"
                }
            };

            var sourceFields = new[]
            {
                new FieldInfo("1", "Control Number", "Fixed-Length Text(250)")
                {
                    IsIdentifier = true
                },
                new FieldInfo("2", "Field 2", "Fixed-Length Text(250)")
            };

            var destinationFields = new[]
            {
                new FieldInfo("10", "Control Number", "Fixed-Length Text(250)")
                {
                    IsIdentifier = true
                },
                new FieldInfo("3", "Field 3", "Fixed-Length Text(250)"),
                new FieldInfo("4", "Field 2", "Fixed-Length Text(250)"),
                new FieldInfo("5", "Field 5", "Fixed-Length Text(250)")
            };

            _viewManagerFake.Setup(x => x.ReadSingleAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new View()
                {
                    Fields = viewFields
                });

            // Act
            var mappedFields = (await _sut.MapFieldsFromViewAsync(sourceFields, destinationFields, DestinationProviderGuid, 1, 2)
                .ConfigureAwait(false)).ToArray();

            // Assert
            mappedFields.Length.Should().Be(2);
            mappedFields[0].SourceField.DisplayName.Should().Be("Control Number");
            mappedFields[1].SourceField.DisplayName.Should().Be("Field 2");
        }

        [Test]
        public void MapFields_ShouldAlwaysSendAutomappedCountMetric()
        {
            // Arrange
            var sourceFields = new[]
            {
                new FieldInfo(fieldIdentifier: "1", name: "Field 1", type: "Fixed-Length Text(250)")
                {
                    IsIdentifier = true
                },
                new FieldInfo(fieldIdentifier: "2", name: "Field 2", type: "Fixed-Length Text(250)")
            };

            var destinationFields = new[]
            {
                new FieldInfo(fieldIdentifier: "3", name: "Field 1", type: "Fixed-Length Text(250)")
                {
                    IsIdentifier = true
                },
                new FieldInfo(fieldIdentifier: "4", name: "Field 2", type: "Fixed-Length Text(50)")
            };

            string bucketName = "FakeProvider.AutoMap.AutoMappedCount";
            _metricBucketNameGeneratorFake.Setup(x => x.GetAutoMapBucketNameAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
                .ReturnsAsync(bucketName);

            // Act
            var mappedFields = _sut.MapFields(sourceFields, destinationFields, DestinationProviderGuid, WorkspaceArtifactId, true).ToArray();

            // Assert
            _metricsSenderMock.Verify(x => x.GaugeOperation(bucketName, mappedFields.Length, It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Test]
        public void MapFields_ShouldSendMapByNameAndDifferentLengthMetric()
        {
            // Arrange
            var sourceFields = new[]
            {
                new FieldInfo(fieldIdentifier: "1", name: "Field 1", type: "Fixed-Length Text(250)")
                {
                    IsIdentifier = true
                },
                new FieldInfo(fieldIdentifier: "2", name: "Field 2", type: "Fixed-Length Text(250)")
            };

            var destinationFields = new[]
            {
                new FieldInfo(fieldIdentifier: "3", name: "Field 1", type: "Fixed-Length Text(250)")
                {
                    IsIdentifier = true
                },
                new FieldInfo(fieldIdentifier: "4", name: "Field 2", type: "Fixed-Length Text(50)")
            };

            string autoMappedByNameCount = "FakeProvider.AutoMap.AutoMappedByNameCount";
            string fixedLengthCount = "FakeProvider.AutoMap.FixedLengthTextTooShortInDestinationCount";
            _metricBucketNameGeneratorFake
                .SetupSequence(x => x.GetAutoMapBucketNameAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
                .ReturnsAsync(autoMappedByNameCount)
                .ReturnsAsync(fixedLengthCount);

            // Act
            var mappedFields = _sut.MapFields(sourceFields, destinationFields, DestinationProviderGuid, WorkspaceArtifactId).ToArray();

            // Assert
            _metricsSenderMock.Verify(x => x.GaugeOperation(autoMappedByNameCount, It.IsAny<long>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()), Times.Once);

            _metricsSenderMock.Verify(x => x.GaugeOperation(fixedLengthCount, It.IsAny<long>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Test]
        public void MapFields_ShouldNotSendMapByNameMetrics_WhenParameterIsSet()
        {
            // Arrange
            var sourceFields = new[]
            {
                new FieldInfo(fieldIdentifier: "1", name: "Field 1", type: "Fixed-Length Text(250)")
                {
                    IsIdentifier = true
                },
                new FieldInfo(fieldIdentifier: "2", name: "Field 2", type: "Fixed-Length Text(250)")
            };

            var destinationFields = new[]
            {
                new FieldInfo(fieldIdentifier: "3", name: "Field 1", type: "Fixed-Length Text(250)")
                {
                    IsIdentifier = true
                },
                new FieldInfo(fieldIdentifier: "4", name: "Field 2", type: "Fixed-Length Text(50)")
            };

            // Act
            var mappedFields = _sut.MapFields(sourceFields, destinationFields, DestinationProviderGuid, WorkspaceArtifactId, true).ToArray();

            // Assert
            _metricsSenderMock.Verify(x => x.GaugeOperation("AutoMappedByNameCount", It.IsAny<long>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()), Times.Never);
        }

    }
}
