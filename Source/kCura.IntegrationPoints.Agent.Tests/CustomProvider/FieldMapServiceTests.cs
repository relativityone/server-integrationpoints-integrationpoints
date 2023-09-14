using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.EntityServices;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.FieldMapping;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Domain.Models;
using Moq;
using NUnit.Framework;
using Relativity;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    [Category("Unit")]
    public class FieldMapServiceTests
    {
        private Mock<IEntityFullNameObjectManagerService> _entityFullNameObjectManagerService;
        private FieldMapService _sut;

        [SetUp]
        public void SetUp()
        {
            _entityFullNameObjectManagerService = new Mock<IEntityFullNameObjectManagerService>();
            _sut = new FieldMapService(_entityFullNameObjectManagerService.Object);
        }

        [Test]
        public async Task GetIdentifierFieldAsync_ShouldReturnDefaultIsIdentifierFieldForDocument()
        {
            // Arrange
            const int workspaceId = 111;
            const int artifactTypeId = (int)ArtifactType.Document;

            IndexedFieldMap identifier = CreateField("ID", true);
            IndexedFieldMap otherField = CreateField("Other", false);

            List<IndexedFieldMap> fieldMap = new List<IndexedFieldMap>()
            {
                identifier,
                otherField
            };

            // Act
            IndexedFieldMap actualIdentifier = await _sut.GetIdentifierFieldAsync(workspaceId, artifactTypeId, fieldMap);

            // Assert
            actualIdentifier.Should().Be(identifier);
            _entityFullNameObjectManagerService.Verify(
                x => x.IsEntityAsync(It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public async Task GetIdentifierFieldAsync_ShouldReturnFullNameInCaseOfEntity()
        {
            // Arrange
            const int workspaceId = 111;
            const int artifactTypeId = 222;

            _entityFullNameObjectManagerService
                .Setup(x => x.IsEntityAsync(workspaceId, artifactTypeId))
                .ReturnsAsync(true);

            IndexedFieldMap uniqueId = CreateField("UniqueID", true);
            IndexedFieldMap fullName = CreateField(EntityFieldNames.FullName, false);

            List<IndexedFieldMap> fieldMap = new List<IndexedFieldMap>()
            {
                uniqueId,
                fullName
            };

            // Act
            IndexedFieldMap actualIdentifier = await _sut.GetIdentifierFieldAsync(workspaceId, artifactTypeId, fieldMap);

            // Assert
            actualIdentifier.Should().Be(fullName);
            _entityFullNameObjectManagerService.Verify(
                x => x.IsEntityAsync(It.IsAny<int>(), It.IsAny<int>()),
                Times.Once);
        }

        [Test]
        public async Task GetIdentifierFieldAsync_ShouldReturnIdFieldInCaseOfOtherObjectTypes()
        {
            // Arrange
            const int workspaceId = 111;
            const int artifactTypeId = 333;

            _entityFullNameObjectManagerService
                .Setup(x => x.IsEntityAsync(workspaceId, artifactTypeId))
                .ReturnsAsync(false);

            IndexedFieldMap identifier = CreateField("ID", true);

            IndexedFieldMap otherField = CreateField("Other", false);

            List<IndexedFieldMap> fieldMap = new List<IndexedFieldMap>()
            {
                identifier,
                otherField
            };

            // Act
            IndexedFieldMap actualIdentifier = await _sut.GetIdentifierFieldAsync(workspaceId, artifactTypeId, fieldMap);

            // Assert
            actualIdentifier.Should().Be(identifier);
            _entityFullNameObjectManagerService.Verify(
                x => x.IsEntityAsync(It.IsAny<int>(), It.IsAny<int>()),
                Times.Once);
        }

        private IndexedFieldMap CreateField(string name, bool isIdentifier)
        {
            FieldMap fieldMap = new FieldMap()
            {
                FieldMapType = isIdentifier ? FieldMapTypeEnum.Identifier : FieldMapTypeEnum.None,
                SourceField = new FieldEntry()
                {
                    IsIdentifier = isIdentifier,
                    DisplayName = name
                },
                DestinationField = new FieldEntry()
                {
                    IsIdentifier = isIdentifier,
                    DisplayName = name
                }
            };

            return new IndexedFieldMap(fieldMap, FieldMapType.Normal, 0);
        }
    }
}