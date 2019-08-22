using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter.Sanitization
{
	[TestFixture]
	internal sealed class ChoiceCacheTests
	{
		private Mock<IChoiceRepository> _choiceRepositoryMock;
		private ChoiceCache _sut;

		[SetUp]
		public void SetUp()
		{
			_choiceRepositoryMock = new Mock<IChoiceRepository>();
			_sut = new ChoiceCache(_choiceRepositoryMock.Object);
		}

		[Test]
		public async Task ItShouldQueryChoiceWithParentUsingChoiceRepository()
		{
			// arrange
			const int choiceArtifactID = 1;
			const int parentArtifactID = 2;
			ChoiceDto choice = new ChoiceDto(choiceArtifactID, string.Empty);
			ChoiceDto parent = new ChoiceDto(parentArtifactID, string.Empty);
			ICollection<ChoiceDto> allChoices = new List<ChoiceDto> { choice, parent };
			var expectedResult = new List<ChoiceWithParentInfoDto>
			{
				new ChoiceWithParentInfoDto(parentArtifactID, choiceArtifactID, string.Empty),
				new ChoiceWithParentInfoDto(null, parentArtifactID, string.Empty)
			};
			const int expectedNumberOfChoices = 2;

			_choiceRepositoryMock
				.Setup(x => x.QueryChoiceWithParentInfoAsync(allChoices, allChoices))
				.ReturnsAsync(expectedResult)
				.Verifiable();

			// act
			IList<ChoiceWithParentInfoDto> choicesWithParentInfo =
				await _sut.GetChoicesWithParentInfoAsync(allChoices).ConfigureAwait(false);

			// assert
			_choiceRepositoryMock.Verify();
			choicesWithParentInfo.Count.Should().Be(expectedNumberOfChoices);
			choicesWithParentInfo.First(x => x.ArtifactID == choiceArtifactID).ParentArtifactID.Should().Be(parentArtifactID);
		}

		[Test]
		public async Task ItShouldReturnChoiceFromCache()
		{
			// arrange
			const int choiceArtifactID = 1;
			const int parentArtifactID = 2;
			ChoiceDto choice = new ChoiceDto(choiceArtifactID, string.Empty);
			ICollection<ChoiceDto> choiceList = new List<ChoiceDto> { choice };
			var expectedResult = new List<ChoiceWithParentInfoDto>
			{
				new ChoiceWithParentInfoDto(parentArtifactID, choiceArtifactID, string.Empty)
			};

			_choiceRepositoryMock
				.Setup(x => x.QueryChoiceWithParentInfoAsync(choiceList, choiceList))
				.ReturnsAsync(expectedResult);

			// act
			IList<ChoiceWithParentInfoDto> firstResult =
				await _sut.GetChoicesWithParentInfoAsync(choiceList).ConfigureAwait(false);
			IList<ChoiceWithParentInfoDto> secondResult =
				await _sut.GetChoicesWithParentInfoAsync(choiceList).ConfigureAwait(false);

			// assert
			secondResult.Should().Equal(firstResult);
			_choiceRepositoryMock.Verify(
				x => x.QueryChoiceWithParentInfoAsync(It.IsAny<ICollection<ChoiceDto>>(), It.IsAny<ICollection<ChoiceDto>>()), Times.Once);
		}
	}
}
