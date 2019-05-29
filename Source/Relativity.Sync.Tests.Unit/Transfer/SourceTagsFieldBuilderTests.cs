using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	[Parallelizable(ParallelScope.All)]
	internal sealed class SourceTagsFieldBuilderTests
	{
		[Test]
		public void ItShouldBuildCorrectColumns()
		{
			// Arrange
			var configuration = new Mock<ISynchronizationConfiguration>();
			var instance = new SourceTagsFieldBuilder(configuration.Object);

			// Act
			List<FieldInfoDto> columns = instance.BuildColumns().ToList();

			// Assert
			const int expectedCount = 2;
			columns.Should().HaveCount(expectedCount);
			columns.Should().Contain(f => f.SpecialFieldType == SpecialFieldType.SourceJob);
			columns.Should().Contain(f => f.SpecialFieldType == SpecialFieldType.SourceWorkspace);
		}

		[Test]
		public async Task ItShouldBuildSpecialFieldRowValueBuilder()
		{
			// Arrange
			var configuration = new Mock<ISynchronizationConfiguration>();
			var instance = new SourceTagsFieldBuilder(configuration.Object);

			const int sourceWorkspaceArtifactId = 1010000;
			const int numDocuments = 100;
			ICollection<int> documentArtifactIds = Enumerable.Range(1, numDocuments).ToList();

			// Act
			ISpecialFieldRowValuesBuilder builder = await instance
				.GetRowValuesBuilderAsync(sourceWorkspaceArtifactId, documentArtifactIds)
				.ConfigureAwait(false);

			// Assert
			builder.Should().NotBeNull();
		}
	}
}
