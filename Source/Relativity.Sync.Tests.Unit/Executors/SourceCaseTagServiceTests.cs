using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public sealed class SourceCaseTagServiceTests
    {
        private Mock<IRelativitySourceCaseTagRepository> _relativitySourceCaseTagRepository;
        private Mock<IWorkspaceNameQuery> _workspaceNameQuery;
        private Mock<IFederatedInstance> _federatedInstance;
        private Mock<ISourceServiceFactoryForUser> _serviceFactoryForUser;

        private SourceCaseTagService _sut;

        [SetUp]
        public void SetUp()
        {
            _relativitySourceCaseTagRepository = new Mock<IRelativitySourceCaseTagRepository>();
            _workspaceNameQuery = new Mock<IWorkspaceNameQuery>();
            _federatedInstance = new Mock<IFederatedInstance>();
            _serviceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();

            var tagNameFormatter = new Mock<ITagNameFormatter>();
            _sut = new SourceCaseTagService(_relativitySourceCaseTagRepository.Object, _workspaceNameQuery.Object, _federatedInstance.Object, tagNameFormatter.Object, _serviceFactoryForUser.Object);
        }

        [Test]
        public async Task ItShouldCreateNewTag()
        {
            const string instanceName = "instance name";
            const string sourceWorkspaceName = "workspace name";
            const int sourceWorkspaceArtifactId = 1;
            const int destinationWorkspaceArtifactId = 2;
            _federatedInstance.Setup(x => x.GetInstanceNameAsync()).ReturnsAsync(instanceName);
            _workspaceNameQuery.Setup(x => x.GetWorkspaceNameAsync(_serviceFactoryForUser.Object, It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(sourceWorkspaceName);

            Mock<IDestinationWorkspaceTagsCreationConfiguration> configuration = new Mock<IDestinationWorkspaceTagsCreationConfiguration>();
            configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(sourceWorkspaceArtifactId);
            configuration.Setup(x => x.DestinationWorkspaceArtifactId).Returns(destinationWorkspaceArtifactId);

            const int tagArtifactId = 4;
            const string tagName = "tagname";
            RelativitySourceCaseTag sourceCaseTag = new RelativitySourceCaseTag()
            {
                ArtifactId = tagArtifactId,
                Name = tagName,
                SourceInstanceName = instanceName,
                SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
                SourceWorkspaceName = sourceWorkspaceName
            };
            _relativitySourceCaseTagRepository.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<RelativitySourceCaseTag>())).ReturnsAsync(sourceCaseTag);

            // act
            RelativitySourceCaseTag createdSourceCaseTag = await _sut.CreateOrUpdateSourceCaseTagAsync(configuration.Object, CancellationToken.None).ConfigureAwait(false);

            // assert
            Assert.AreEqual(tagArtifactId, createdSourceCaseTag.ArtifactId);
            Assert.AreEqual(tagName, createdSourceCaseTag.Name);
            Assert.AreEqual(instanceName, createdSourceCaseTag.SourceInstanceName);
            Assert.AreEqual(sourceWorkspaceArtifactId, createdSourceCaseTag.SourceWorkspaceArtifactId);
            Assert.AreEqual(sourceWorkspaceName, createdSourceCaseTag.SourceWorkspaceName);
        }

        [Test]
        public async Task ItShouldUpdateExistingTag()
        {
            const string instanceName = "instance name";
            const int sourceWorkspaceArtifactId = 1;
            const int destinationWorkspaceArtifactId = 2;
            const string newWorkspaceName = "workspace name";
            _federatedInstance.Setup(x => x.GetInstanceNameAsync()).ReturnsAsync(instanceName);
            _workspaceNameQuery.Setup(x => x.GetWorkspaceNameAsync(_serviceFactoryForUser.Object, It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(newWorkspaceName);

            Mock<IDestinationWorkspaceTagsCreationConfiguration> configuration = new Mock<IDestinationWorkspaceTagsCreationConfiguration>();
            configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(sourceWorkspaceArtifactId);
            configuration.Setup(x => x.DestinationWorkspaceArtifactId).Returns(destinationWorkspaceArtifactId);

            const int tagArtifactId = 4;
            const string oldTagName = "old tagname";
            const string oldWSourceWorkspaceName = "old workspace";
            RelativitySourceCaseTag sourceCaseTag = new RelativitySourceCaseTag()
            {
                ArtifactId = tagArtifactId,
                Name = oldTagName,
                SourceInstanceName = instanceName,
                SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
                SourceWorkspaceName = oldWSourceWorkspaceName
            };
            _relativitySourceCaseTagRepository.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), CancellationToken.None)).ReturnsAsync(sourceCaseTag);

            // act
            await _sut.CreateOrUpdateSourceCaseTagAsync(configuration.Object, CancellationToken.None).ConfigureAwait(false);

            // assert
            _relativitySourceCaseTagRepository.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<RelativitySourceCaseTag>()));
        }
    }
}