using System.Collections.Generic;
using FluentAssertions;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Domain.Models;
using Moq;
using NUnit.Framework;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using SyncFieldMap = Relativity.Sync.Storage.FieldMap;
using SyncFieldEntry = Relativity.Sync.Storage.FieldEntry;

namespace kCura.IntegrationPoints.RelativitySync.Tests
{
    [TestFixture, Category("Unit")]
    internal sealed class SyncFieldMapConverterTests
    {
        private SyncFieldMapConverter _sut;

        [SetUp]
        public void SetUp()
        {
            var loggerFake = new Mock<ILogger<SyncFieldMapConverter>>();
            _sut = new SyncFieldMapConverter(loggerFake.Object);
        }

        [Test]
        public void FixedSyncMapping_ShouldHandleEmptyMapping()
        {
            // Act
            List<SyncFieldMap> fieldsMapping = _sut.ConvertToSyncFieldMap(new List<FieldMap>());

            // Assert
            fieldsMapping.Should().BeEmpty();
        }

        [Test]
        public void FixedSyncMapping_ShouldRemoveSuffixFromIdentifierMapping()
        {
            // Arrange
            List<FieldMap> fieldMap = new List<FieldMap>
            {
                new FieldMap
                {
                    DestinationField = new FieldEntry
                    {
                        FieldIdentifier = "1",
                        DisplayName = "abc [Object Identifier]",
                        IsIdentifier = true
                    },
                    SourceField = new FieldEntry
                    {
                        FieldIdentifier = "1",
                        DisplayName = "abc [Object Identifier]",
                        IsIdentifier = true
                    },
                    FieldMapType = FieldMapTypeEnum.Identifier
                }
            };

            List<SyncFieldMap> expectedFieldsMapping = new List<SyncFieldMap>
            {
                new SyncFieldMap
                {
                    DestinationField = new SyncFieldEntry
                    {
                        FieldIdentifier = 1,
                        DisplayName = "abc",
                        IsIdentifier = true
                    },
                    SourceField = new SyncFieldEntry
                    {
                        FieldIdentifier = 1,
                        DisplayName = "abc",
                        IsIdentifier = true
                    },
                }
            };

            // Act
            List<SyncFieldMap> fieldsMapping = _sut.ConvertToSyncFieldMap(fieldMap);

            // Assert
            fieldsMapping.ShouldBeEquivalentTo(expectedFieldsMapping);
        }

        [Test]
        public void FixedSyncMapping_ShouldRemoveSpecialFields()
        {
            // Arrange
            const FieldMapTypeEnum folderPathTypeEnum = FieldMapTypeEnum.FolderPathInformation;
            const FieldMapTypeEnum nativeFileTypeEnum = FieldMapTypeEnum.NativeFilePath;
            const FieldMapTypeEnum fieldMapTypeNotToBeRemoved = FieldMapTypeEnum.Identifier;
            List<FieldMap> fieldMap = new List<FieldMap>
            {
                new FieldMap
                {
                    SourceField = new FieldEntry
                    {
                        FieldIdentifier = "1",
                        DisplayName = "Field"
                    },
                    FieldMapType = folderPathTypeEnum
                },
                new FieldMap
                {
                    DestinationField = new FieldEntry
                    {
                        FieldIdentifier = "2",
                        DisplayName = "field"
                    },
                    SourceField = new FieldEntry
                    {
                        FieldIdentifier = "2",
                        DisplayName = "Field"
                    },
                    FieldMapType = nativeFileTypeEnum
                },
                new FieldMap
                {
                    DestinationField = new FieldEntry
                    {
                        FieldIdentifier = "3",
                        DisplayName = "field"
                    },
                    SourceField = new FieldEntry
                    {
                        FieldIdentifier = "3",
                        DisplayName = "Field"
                    },
                    FieldMapType = fieldMapTypeNotToBeRemoved
                }
            };

            List<SyncFieldMap> expectedFieldsMapping = new List<SyncFieldMap>
            {
                new SyncFieldMap
                {
                    DestinationField = new SyncFieldEntry
                    {
                        FieldIdentifier = 3,
                        DisplayName = "field",
                        IsIdentifier = false
                    },
                    SourceField = new SyncFieldEntry
                    {
                        FieldIdentifier = 3,
                        DisplayName = "Field",
                        IsIdentifier = false
                    },
                }
            };

            // Act
            List<SyncFieldMap> fieldsMapping = _sut.ConvertToSyncFieldMap(fieldMap);

            // Assert
            fieldsMapping.ShouldBeEquivalentTo(expectedFieldsMapping);
        }

        [Test]
        public void FixedSyncMapping_ShouldDeduplicateFields()
        {
            // Arrange
            const string uniqueFieldIdentifier = "111";
            const string duplicatedFieldIdentifier = "222";

            List<FieldMap> fieldMap = new List<FieldMap>
            {
                new FieldMap
                {
                    SourceField = new FieldEntry
                    {
                        DisplayName = "unique field",
                        FieldIdentifier = uniqueFieldIdentifier
                    },
                    DestinationField = new FieldEntry
                    {
                        DisplayName = "unique field",
                        FieldIdentifier = uniqueFieldIdentifier
                    }
                },
                new FieldMap()
                {
                    SourceField = new FieldEntry()
                    {
                        DisplayName = "duplicated field",
                        FieldIdentifier = duplicatedFieldIdentifier
                    },
                    DestinationField = new FieldEntry()
                    {
                        DisplayName = "duplicated field",
                        FieldIdentifier = duplicatedFieldIdentifier
                    }
                },
                new FieldMap()
                {
                    SourceField = new FieldEntry()
                    {
                        DisplayName = "duplicated field",
                        FieldIdentifier = duplicatedFieldIdentifier
                    },
                    DestinationField = new FieldEntry()
                    {
                        DisplayName = "duplicated field",
                        FieldIdentifier = duplicatedFieldIdentifier
                    }
                }
            };

            List<SyncFieldMap> expectedFieldsMapping = new List<SyncFieldMap>
            {
                new SyncFieldMap
                {
                    SourceField = new SyncFieldEntry
                    {
                        DisplayName = "unique field",
                        FieldIdentifier = int.Parse(uniqueFieldIdentifier)
                    },
                    DestinationField = new SyncFieldEntry
                    {
                        DisplayName = "unique field",
                        FieldIdentifier = int.Parse(uniqueFieldIdentifier)
                    }
                },
                new SyncFieldMap()
                {
                    SourceField = new SyncFieldEntry()
                    {
                        DisplayName = "duplicated field",
                        FieldIdentifier = int.Parse(duplicatedFieldIdentifier)
                    },
                    DestinationField = new SyncFieldEntry()
                    {
                        DisplayName = "duplicated field",
                        FieldIdentifier = int.Parse(duplicatedFieldIdentifier)
                    }
                }
            };

            // Act
            List<SyncFieldMap> fieldsMapping = _sut.ConvertToSyncFieldMap(fieldMap);

            // Assert
            fieldsMapping.ShouldBeEquivalentTo(expectedFieldsMapping);
        }
    }
}
