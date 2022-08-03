using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.RelativitySync.Utils;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

using SyncFieldMap = Relativity.Sync.Storage.FieldMap;
using SyncFieldEntry = Relativity.Sync.Storage.FieldEntry;
using SyncFieldMapType = Relativity.Sync.Storage.FieldMapType;

namespace kCura.IntegrationPoints.RelativitySync.Tests
{
    [TestFixture, Category("Unit")]
    internal sealed class FieldMapHelperTests
    {
        private ISerializer _serializer;
        private Mock<IAPILog> _loggerFake;

        [SetUp]
        public void SetUp()
        {
            _serializer = new JSONSerializer();
            _loggerFake = new Mock<IAPILog>();
        }

        [Test]
        public void FixedSyncMapping_ShouldHandleEmptyMapping()
        {
            // Act
            List<SyncFieldMap> fieldsMapping = FieldMapHelper.FixedSyncMapping(string.Empty, _serializer, _loggerFake.Object);

            // Assert
            fieldsMapping.Should().BeEmpty();
        }

        [Test]
        public void FixedSyncMapping_ShouldHandleMappingWithoutIdentifier()
        {
            // Arrange
            List<FieldMap> fieldMap = new List<FieldMap>
            {
                new FieldMap
                {
                    DestinationField = new FieldEntry
                    {
                        FieldIdentifier = "1",
                        DisplayName = "abc"
                    },
                    SourceField = new FieldEntry
                    {
                        FieldIdentifier = "1",
                        DisplayName = "abc [Object Identifier]"
                    },
                    FieldMapType = FieldMapTypeEnum.None
                }
            };

            List<SyncFieldMap> expectedFieldsMapping = new List<SyncFieldMap>
            {
                new SyncFieldMap
                {
                    DestinationField = new SyncFieldEntry
                    {
                        FieldIdentifier = 1,
                        DisplayName = "abc"
                    },
                    SourceField = new SyncFieldEntry
                    {
                        FieldIdentifier = 1,
                        DisplayName = "abc [Object Identifier]"
                    },
                    FieldMapType = SyncFieldMapType.None
                }
            };

            string fieldMapping = _serializer.Serialize(fieldMap);

            // Act
            List<SyncFieldMap> fieldsMapping = FieldMapHelper.FixedSyncMapping(fieldMapping, _serializer, _loggerFake.Object);

            // Assert
            fieldsMapping.ShouldBeEquivalentTo(expectedFieldsMapping);
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
                    },
                    SourceField = new FieldEntry
                    {
                        FieldIdentifier = "1",
                        DisplayName = "abc [Object Identifier]",
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
                        DisplayName = "abc"
                    },
                    SourceField = new SyncFieldEntry
                    {
                        FieldIdentifier = 1,
                        DisplayName = "abc"
                    },
                    FieldMapType = SyncFieldMapType.Identifier
                }
            };

            string fieldMapping = _serializer.Serialize(fieldMap);

            // Act
            List<SyncFieldMap> fieldsMapping = FieldMapHelper.FixedSyncMapping(fieldMapping, _serializer, _loggerFake.Object);

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
                        DisplayName = "field"
                    },
                    SourceField = new SyncFieldEntry
                    {
                        FieldIdentifier = 3,
                        DisplayName = "Field"
                    },
                    FieldMapType = fieldMapTypeNotToBeRemoved.ToSyncFieldMapType()
                }
            };

            string fieldMapping = _serializer.Serialize(fieldMap);

            // Act
            List<SyncFieldMap> fieldsMapping = FieldMapHelper.FixedSyncMapping(fieldMapping, _serializer, _loggerFake.Object);

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

            string fieldMapping = _serializer.Serialize(fieldMap);

            // Act
            List<SyncFieldMap> fieldsMapping = FieldMapHelper.FixedSyncMapping(fieldMapping, _serializer, _loggerFake.Object);

            // Assert
            fieldsMapping.ShouldBeEquivalentTo(expectedFieldsMapping);
        }
    }
}