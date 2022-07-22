using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    internal sealed class ChoiceCacheTests
    {
        private Mock<ISynchronizationConfiguration> _config;
        private Mock<ISourceServiceFactoryForUser> _serviceFactoryForUser;
        private Mock<IObjectManager> _objectManager;
        private ChoiceCache _instance;

        [SetUp]
        public void SetUp()
        {
            _config = new Mock<ISynchronizationConfiguration>();
            _serviceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
            _objectManager = new Mock<IObjectManager>();
            _serviceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
            _instance = new ChoiceCache(_config.Object, _serviceFactoryForUser.Object);
        }

        [Test]
        public async Task ItShouldQueryChoiceUsingObjectManager()
        {
            const int choiceArtifactId = 1;
            const int parentArtifactId = 2;
            Choice choice = new Choice()
            {
                ArtifactID = choiceArtifactId
            };
            QueryResult queryResult = new QueryResult()
            {
                Objects = new List<RelativityObject>
                {
                    new RelativityObject()
                    {
                        ArtifactID = choiceArtifactId,
                        ParentObject = new RelativityObjectRef()
                        {
                            ArtifactID = parentArtifactId
                        }
                    }
                } 
            };
            _objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult)
                .Verifiable();

            // act
            IList<ChoiceWithParentInfo> choicesWithParentInfo = await _instance.GetChoicesWithParentInfoAsync(new List<Choice>() {choice}).ConfigureAwait(false);

            // assert
            _objectManager.Verify();
            choicesWithParentInfo.Count.Should().Be(1);
            choicesWithParentInfo.First().ArtifactID.Should().Be(choiceArtifactId);
            choicesWithParentInfo.First().ParentArtifactId.Should().BeNull();
        }

        [Test]
        public async Task ItShouldQueryChoiceWithParentUsingObjectManager()
        {
            const int choiceArtifactId = 1;
            const int parentArtifactId = 2;
            Choice choice = new Choice()
            {
                ArtifactID = choiceArtifactId
            };
            Choice parent = new Choice()
            {
                ArtifactID = parentArtifactId
            };
            QueryResult queryResult = new QueryResult()
            {
                Objects = new List<RelativityObject>
                {
                    new RelativityObject()
                    {
                        ArtifactID = choiceArtifactId,
                        ParentObject = new RelativityObjectRef()
                        {
                            ArtifactID = parentArtifactId
                        }
                    }
                }
            };
            _objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult)
                .Verifiable();

            // act
            IList<ChoiceWithParentInfo> choicesWithParentInfo = await _instance.GetChoicesWithParentInfoAsync(new List<Choice>() { choice, parent }).ConfigureAwait(false);

            // assert
            _objectManager.Verify();
            const int numberOfChoices = 2;
            choicesWithParentInfo.Count.Should().Be(numberOfChoices);
            choicesWithParentInfo.First(x => x.ArtifactID == choiceArtifactId).ParentArtifactId.Should().Be(parentArtifactId);
        }

        [Test]
        public async Task ItShouldReturnChoiceFromCache()
        {
            const int choiceArtifactId = 1;
            const int parentArtifactId = 2;
            Choice choice = new Choice()
            {
                ArtifactID = choiceArtifactId
            };
            QueryResult queryResult = new QueryResult()
            {
                Objects = new List<RelativityObject>
                {
                    new RelativityObject()
                    {
                        ArtifactID = choiceArtifactId,
                        ParentObject = new RelativityObjectRef()
                        {
                            ArtifactID = parentArtifactId
                        }
                    }
                }
            };
            _objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult)
                .Verifiable();

            // act
            await _instance.GetChoicesWithParentInfoAsync(new List<Choice>() { choice }).ConfigureAwait(false);
            await _instance.GetChoicesWithParentInfoAsync(new List<Choice>() { choice }).ConfigureAwait(false);

            // assert
            _objectManager.Verify(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }
    }
}