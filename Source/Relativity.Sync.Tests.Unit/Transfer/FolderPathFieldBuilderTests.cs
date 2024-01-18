using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    internal sealed class FolderPathFieldBuilderTests
    {
        private static IEnumerable<TestCaseData> BuildColumnsTestCases()
        {
            yield return new TestCaseData(
                DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure,
                FieldInfoDto.FolderPathFieldFromSourceWorkspaceStructure());
            yield return new TestCaseData(
                DestinationFolderStructureBehavior.ReadFromField,
                FieldInfoDto.FolderPathFieldFromDocumentField("foo"));
        }

        [TestCaseSource(nameof(BuildColumnsTestCases))]
        public void ItShouldBuildCorrectColumn(DestinationFolderStructureBehavior folderStructureBehavior, FieldInfoDto expected)
        {
            // Arrange
            var folderPathRetriever = new Mock<IFolderPathRetriever>();

            var fieldConfiguration = new Mock<IFieldConfiguration>();
            fieldConfiguration.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(folderStructureBehavior);
            fieldConfiguration.Setup(x => x.GetFolderPathSourceFieldName()).Returns("foo");

            var instance = new FolderPathFieldBuilder(folderPathRetriever.Object, fieldConfiguration.Object);

            // Act
            List<FieldInfoDto> columns = instance.BuildColumns().ToList();

            // Assert
            columns.Should().ContainSingle().Which.Should().BeEquivalentTo(expected);
        }
    }
}
