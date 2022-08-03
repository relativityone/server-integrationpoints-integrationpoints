using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class ChoiceRepositoryTests
    {
        private Mock<IRelativityObjectManager> _objectManagerMock;
        private ChoiceRepository _sut;

        [SetUp]
        public void SetUp()
        {
            _objectManagerMock = new Mock<IRelativityObjectManager>();
            _sut = new ChoiceRepository(_objectManagerMock.Object);
        }

        [Test]
        public async Task ItShouldQueryChoiceUsingObjectManager()
        {
            // arrange
            const int choiceArtifactID = 1;
            const int parentArtifactID = 2;
            ChoiceDto choice = new ChoiceDto(choiceArtifactID, string.Empty);
            ICollection<ChoiceDto> choiceList = new List<ChoiceDto> { choice };
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
            IList<ChoiceWithParentInfoDto> choicesWithParentInfo =
                await _sut.QueryChoiceWithParentInfoAsync(choiceList, choiceList).ConfigureAwait(false);

            // assert
            _objectManagerMock.Verify();
            choicesWithParentInfo.Count.Should().Be(1);
            choicesWithParentInfo.First().ArtifactID.Should().Be(choiceArtifactID);
            choicesWithParentInfo.First().ParentArtifactID.Should().BeNull();
        }

        [Test]
        public async Task ItShouldQueryChoiceWithParentUsingObjectManager()
        {
            // arrange
            const int choiceArtifactID = 1;
            const int parentArtifactID = 2;
            ChoiceDto choice = new ChoiceDto(choiceArtifactID, string.Empty);
            ChoiceDto parent = new ChoiceDto(parentArtifactID, string.Empty);
            ICollection<ChoiceDto> choiceList = new List<ChoiceDto> {choice};
            ICollection<ChoiceDto> allChoices = new List<ChoiceDto> {choice, parent};
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
            IList<ChoiceWithParentInfoDto> choicesWithParentInfo =
                await _sut.QueryChoiceWithParentInfoAsync(choiceList, allChoices).ConfigureAwait(false);

            // assert
            _objectManagerMock.Verify();
            choicesWithParentInfo.Count.Should().Be(1);
            choicesWithParentInfo.First().ArtifactID.Should().Be(choiceArtifactID);
            choicesWithParentInfo.First().ParentArtifactID.Should().Be(parentArtifactID);
        }

        [Test]
        public async Task ItShouldQueryMultipleChoicesUsingObjectManager()
        {
            // arrange
            const int childArtifactID = 1;
            const int parentArtifactID = 2;
            const int singleArtifactID = 3;
            const int otherArtifactID = 4;
            ChoiceDto child = new ChoiceDto(childArtifactID, string.Empty);
            ChoiceDto parent = new ChoiceDto(parentArtifactID, string.Empty);
            ChoiceDto single = new ChoiceDto(singleArtifactID, string.Empty);
            ICollection<ChoiceDto> choiceList = new List<ChoiceDto> { child, single };
            ICollection<ChoiceDto> allChoices = new List<ChoiceDto> { child, parent, single };
            List<RelativityObject> queryResult = new List<RelativityObject>
            {
                new RelativityObject
                {
                    ArtifactID = childArtifactID,
                    ParentObject = new RelativityObjectRef
                    {
                        ArtifactID = parentArtifactID
                    }
                },
                new RelativityObject
                {
                    ArtifactID = singleArtifactID,
                    ParentObject = new RelativityObjectRef
                    {
                        ArtifactID = otherArtifactID
                    }
                }
            };

            _objectManagerMock
                .Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<ExecutionIdentity>()))
                .ReturnsAsync(queryResult)
                .Verifiable();

            // act
            IList<ChoiceWithParentInfoDto> choicesWithParentInfo =
                await _sut.QueryChoiceWithParentInfoAsync(choiceList, allChoices).ConfigureAwait(false);

            // assert
            _objectManagerMock.Verify();
            choicesWithParentInfo.First(x => x.ArtifactID == childArtifactID).ParentArtifactID.Should().Be(parentArtifactID);
            choicesWithParentInfo.First(x => x.ArtifactID == singleArtifactID).ParentArtifactID.Should().Be(null);
        }
    }
}
