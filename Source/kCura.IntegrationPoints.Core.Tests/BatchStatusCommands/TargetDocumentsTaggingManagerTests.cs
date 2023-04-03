using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.BatchStatusCommands
{
    [TestFixture, Category("Unit")]
    public class TargetDocumentsTaggingManagerTests : TestBase
    {
        private IRepositoryFactory _repositoryFactory;
        private ITagsCreator _tagsCreator;
        private ITagger _tagger;
        private ITagSavedSearchManager _tagSavedSearchManager;
        private IAPILog _logger;
        private IScratchTableRepository _scratchTableRepository;
        private int _sourceWorkspaceArtifactId;
        private int _destinationWorkspaceArtifactId;
        private int _jobHistoryArtifactId;
        private int? _federatedInstanceArtifactId;
        private DestinationConfiguration _destinationConfiguration;
        private readonly string _uniqueJobId = "1_JobIdGuid";
        private TargetDocumentsTaggingManager _instance;

        [SetUp]
        public override void SetUp()
        {
            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _tagsCreator = Substitute.For<ITagsCreator>();
            _tagger = Substitute.For<ITagger>();
            _tagSavedSearchManager = Substitute.For<ITagSavedSearchManager>();
            _logger = Substitute.For<IAPILog>();
            _scratchTableRepository = Substitute.For<IScratchTableRepository>();

            _destinationConfiguration = new DestinationConfiguration();
            _sourceWorkspaceArtifactId = 320187;
            _destinationWorkspaceArtifactId = 648827;
            _federatedInstanceArtifactId = null;
            _jobHistoryArtifactId = 151262;

            _repositoryFactory.GetScratchTableRepository(_sourceWorkspaceArtifactId, Data.Constants.TEMPORARY_DOC_TABLE_SOURCEWORKSPACE, _uniqueJobId)
                .ReturnsForAnyArgs(_scratchTableRepository);

            var helper = Substitute.For<IHelper>();
            helper.GetLoggerFactory().GetLogger().ForContext<TargetDocumentsTaggingManager>().Returns(_logger);

            _instance = new TargetDocumentsTaggingManager(_repositoryFactory, _tagsCreator,
                _tagger, _tagSavedSearchManager, helper, new ImportSettings(_destinationConfiguration), _sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId,
                _federatedInstanceArtifactId, _jobHistoryArtifactId, _uniqueJobId);
        }

        [Test]
        public void ItShouldCreateTagsOnJobStart()
        {
            // ACT
            _instance.OnJobStart(null);

            // ASSERT
            _tagsCreator.Received(1).CreateTags(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId, _jobHistoryArtifactId, _federatedInstanceArtifactId);
        }

        [Test]
        public void ItShouldLogErrorOnJobStart()
        {
            _tagsCreator.When(x => x.CreateTags(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId, _jobHistoryArtifactId, _federatedInstanceArtifactId))
                .Do(x => { throw new Exception(); });

            // ACT
            Assert.That(() => _instance.OnJobStart(null), Throws.Exception);

            // ASSERT
            _logger.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>());
        }

        [Test]
        public void ItShouldLogErrorOnJobComplete()
        {
            _tagger.When(x => x.TagDocuments(Arg.Any<TagsContainer>(), _scratchTableRepository))
                .Do(x => { throw new Exception(); });

            // ACT
            Assert.That(() => _instance.OnJobComplete(null), Throws.Exception);

            // ASSERT
            _logger.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>());
        }

        [Test]
        public void ItShouldDisposeScratchTableOnJobComplete()
        {
            _tagger.When(x => x.TagDocuments(Arg.Any<TagsContainer>(), _scratchTableRepository))
                .Do(x => { throw new Exception(); });

            // ACT
            Assert.That(() => _instance.OnJobComplete(null), Throws.Exception);

            // ASSERT
            _scratchTableRepository.Received(1).Dispose();
        }

        [Test]
        public void ItShouldTagDocumentsOnJobComplete()
        {
            var tagsContainer = new TagsContainer(null, null);
            _tagsCreator.CreateTags(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId, _jobHistoryArtifactId, _federatedInstanceArtifactId).Returns(tagsContainer);

            _instance.OnJobStart(null);

            // ACT
            _instance.OnJobComplete(null);

            // ASSERT
            _tagger.Received(1).TagDocuments(tagsContainer, _scratchTableRepository);
        }

        [Test]
        public void ItShouldCreateSavedSearchForTaggingOnJobComplete()
        {
            var tagsContainer = new TagsContainer(null, null);
            _tagsCreator.CreateTags(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId, _jobHistoryArtifactId, _federatedInstanceArtifactId).Returns(tagsContainer);

            _instance.OnJobStart(null);

            // ACT
            _instance.OnJobComplete(null);

            // ASSERT
            _tagSavedSearchManager.Received(1).CreateSavedSearchForTagging(_destinationWorkspaceArtifactId, _destinationConfiguration, tagsContainer);
        }

        [Test]
        public void ItShouldSkipTaggingOnJobCompleteWhenErrorOccuredDuringJobStart()
        {
            _tagsCreator.When(x => x.CreateTags(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId, _jobHistoryArtifactId, _federatedInstanceArtifactId))
                .Do(x => { throw new Exception(); });

            try
            {
                _instance.OnJobStart(null);
            }
            catch
            {
                // ignore
            }

            // ACT
            _instance.OnJobComplete(null);

            // ASSERT
            _tagger.DidNotReceiveWithAnyArgs().TagDocuments(Arg.Any<TagsContainer>(), Arg.Any<IScratchTableRepository>());
            _tagSavedSearchManager.DidNotReceiveWithAnyArgs().CreateSavedSearchForTagging(Arg.Any<int>(), Arg.Any<DestinationConfiguration>(), Arg.Any<TagsContainer>());
        }
    }
}
