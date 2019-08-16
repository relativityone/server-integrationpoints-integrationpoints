using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using kCura.IntegrationPoints.Data.Repositories;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter.Sanitization
{
	[TestFixture]
	internal sealed class ChoiceCacheTests
	{
		private Mock<IRelativityObjectManager> _objectManagerMock;
		private ChoiceCache _sut;

		[SetUp]
		public void SetUp()
		{
			_objectManagerMock = new Mock<IRelativityObjectManager>();
			_sut = new ChoiceCache(_objectManagerMock.Object);
		}

		[Test]
		public async Task ItShouldQueryChoiceUsingObjectManager()
		{
			// arrange
			const int choiceArtifactID = 1;
			const int parentArtifactID = 2;
			Choice choice = new Choice
			{
				ArtifactID = choiceArtifactID
			};
			List<RelativityObject> queryResult = new List<RelativityObject>
			{
				new RelativityObject
				{
					ArtifactID = choiceArtifactID,
					ParentObject = new RelativityObjectRef
					{
						ArtifactID = parentArtifactID
					}
				}
			};

			_objectManagerMock
				.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<ExecutionIdentity>()))
				.ReturnsAsync(queryResult)
				.Verifiable();

			// act
			IList<ChoiceWithParentInfo> choicesWithParentInfo =
				await _sut.GetChoicesWithParentInfoAsync(new List<Choice> {choice}).ConfigureAwait(false);

			// assert
			_objectManagerMock.Verify();
			choicesWithParentInfo.Count.Should().Be(1);
			choicesWithParentInfo.First().ArtifactID.Should().Be(choiceArtifactID);
			choicesWithParentInfo.First().ParentArtifactID.Should().BeNull();
		}

		[Test]
		public async Task ItShouldQueryChoiceWithParentUsingObjectManager()
		{
			const int choiceArtifactID = 1;
			const int parentArtifactID = 2;
			Choice choice = new Choice
			{
				ArtifactID = choiceArtifactID
			};
			Choice parent = new Choice
			{
				ArtifactID = parentArtifactID
			};
			List<RelativityObject> queryResult = new List<RelativityObject>
			{
				new RelativityObject
				{
					ArtifactID = choiceArtifactID,
					ParentObject = new RelativityObjectRef
					{
						ArtifactID = parentArtifactID
					}
				}
			};

			_objectManagerMock
				.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<ExecutionIdentity>()))
				.ReturnsAsync(queryResult)
				.Verifiable();

			// act
			IList<ChoiceWithParentInfo> choicesWithParentInfo =
				await _sut.GetChoicesWithParentInfoAsync(new List<Choice> {choice, parent}).ConfigureAwait(false);

			// assert
			_objectManagerMock.Verify();
			const int numberOfChoices = 2;
			choicesWithParentInfo.Count.Should().Be(numberOfChoices);
			choicesWithParentInfo.First(x => x.ArtifactID == choiceArtifactID).ParentArtifactID.Should().Be(parentArtifactID);
		}

		[Test]
		public async Task ItShouldReturnChoiceFromCache()
		{
			const int choiceArtifactID = 1;
			const int parentArtifactID = 2;
			Choice choice = new Choice
			{
				ArtifactID = choiceArtifactID
			};
			List<RelativityObject> queryResult = new List<RelativityObject>
			{
				new RelativityObject
				{
					ArtifactID = choiceArtifactID,
					ParentObject = new RelativityObjectRef
					{
						ArtifactID = parentArtifactID
					}
				}
			};

			_objectManagerMock
				.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<ExecutionIdentity>()))
				.ReturnsAsync(queryResult)
				.Verifiable();

			// act
			await _sut.GetChoicesWithParentInfoAsync(new List<Choice> { choice }).ConfigureAwait(false);
			await _sut.GetChoicesWithParentInfoAsync(new List<Choice> { choice }).ConfigureAwait(false);

			// assert
			_objectManagerMock.Verify(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<ExecutionIdentity>()), Times.Once);
		}
	}
}
