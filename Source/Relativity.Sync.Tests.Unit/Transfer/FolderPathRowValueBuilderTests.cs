using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    internal sealed class FolderPathRowValueBuilderTests
    {
        private static readonly IDictionary<int, string> DefaultFolderPathsMap = new Dictionary<int, string>
        {
            { 0, "foo\\bar" },
            { 1, "bat\\baz\\bang" }
        };

        [TestCase(DestinationFolderStructureBehavior.None)]
        [TestCase(DestinationFolderStructureBehavior.ReadFromField)]
        public void ItShouldReturnInitialObjectWhenNotRetainingSourceWorkspaceStructure(DestinationFolderStructureBehavior folderStructureBehavior)
        {
            FieldInfoDto fieldInfo = FieldInfoDto.GenericSpecialField(SpecialFieldType.FolderPath, "blah", "blah");
            RelativityObjectSlim document = new RelativityObjectSlim { ArtifactID = 1 };
            string initialValue = "test\\test";

            FolderPathRowValueBuilder instance = new FolderPathRowValueBuilder(folderStructureBehavior, DefaultFolderPathsMap);

            // Act
            object result = instance.BuildRowValue(fieldInfo, document, initialValue);

            // Assert
            result.Should().Be(initialValue);
        }

        [Test]
        public void ItShouldReturnFolderPathByArtifactIdWhenRetainingSourceWorkspaceStructure()
        {
            FieldInfoDto fieldInfo = FieldInfoDto.GenericSpecialField(SpecialFieldType.FolderPath, "blah", "blah");
            RelativityObjectSlim document = new RelativityObjectSlim { ArtifactID = 1 };
            string initialValue = "test\\test";

            FolderPathRowValueBuilder instance = new FolderPathRowValueBuilder(DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure, DefaultFolderPathsMap);

            // Act
            object result = instance.BuildRowValue(fieldInfo, document, initialValue);

            // Assert
            result.Should().Be(DefaultFolderPathsMap[document.ArtifactID]);
        }

        [Test]
        public void BuildRowValue_ShouldThrowSyncItemLevelErrorException_WhenCouldNotFindFolderWithGivenID()
        {
            FieldInfoDto fieldInfo = FieldInfoDto.FolderPathFieldFromDocumentField("name");
            RelativityObjectSlim document = new RelativityObjectSlim { ArtifactID = 3 };
            string initialValue = "test\\test";

            FolderPathRowValueBuilder instance = new FolderPathRowValueBuilder(DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure, DefaultFolderPathsMap);

            // Act
            Action action = () => instance.BuildRowValue(fieldInfo, document, initialValue);

            // Assert
            action.Should().Throw<SyncItemLevelErrorException>();
        }

        [Test]
        public void ItShouldThrowArgumentExceptionWhenNonFolderPathFieldIsGiven()
        {
            FieldInfoDto fieldInfo = FieldInfoDto.NativeFileLocationField();
            RelativityObjectSlim document = new RelativityObjectSlim { ArtifactID = 1 };
            string initialValue = "test\\test";

            FolderPathRowValueBuilder instance = new FolderPathRowValueBuilder(DestinationFolderStructureBehavior.None, DefaultFolderPathsMap);

            // Act
            Action action = () => instance.BuildRowValue(fieldInfo, document, initialValue);

            // Assert
            action.Should().Throw<ArgumentException>();
        }
    }
}
