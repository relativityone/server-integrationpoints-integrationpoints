using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Core.Tests.Tagging
{
    [TestFixture, Category("Unit")]
    public class TagSavedSearchFolderTests : TestBase
    {
        private const int _WORKSPACE_ID = 513406;
        private IKeywordSearchRepository _keywordSearchRepository;
        private TagSavedSearchFolder _instance;

        public override void SetUp()
        {
            _keywordSearchRepository = Substitute.For<IKeywordSearchRepository>();

            var repositoryFactory = Substitute.For<IRepositoryFactory>();
            repositoryFactory.GetKeywordSearchRepository().Returns(_keywordSearchRepository);

            IHelper helper = Substitute.For<IHelper>();

            _instance = new TagSavedSearchFolder(repositoryFactory, helper);
        }

        [Test]
        public void ItShouldReturnExistingFolder()
        {
            var searchContainer = new SearchContainer
            {
                ArtifactID = 128185
            };

            _keywordSearchRepository.QuerySearchContainer(_WORKSPACE_ID, Data.Constants.DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME).Returns(searchContainer);

            // ACT
            var actualFolderId = _instance.GetFolderId(_WORKSPACE_ID);

            // ASSERT
            Assert.That(actualFolderId, Is.EqualTo(searchContainer.ArtifactID));
            _keywordSearchRepository.Received(1).QuerySearchContainer(_WORKSPACE_ID, Data.Constants.DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME);
            _keywordSearchRepository.DidNotReceiveWithAnyArgs().CreateSearchContainerInRoot(_WORKSPACE_ID, Arg.Any<string>());
        }

        [Test]
        public void ItShouldCreateFolderIfNotFound()
        {
            var expectedFolderId = 426274;

            _keywordSearchRepository.QuerySearchContainer(_WORKSPACE_ID, Data.Constants.DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME).Returns((SearchContainer) null);

            _keywordSearchRepository.CreateSearchContainerInRoot(_WORKSPACE_ID, Data.Constants.DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME).Returns(expectedFolderId);

            // ACT
            var actualFolderId = _instance.GetFolderId(_WORKSPACE_ID);

            // ASSERT
            Assert.That(actualFolderId, Is.EqualTo(expectedFolderId));
            _keywordSearchRepository.Received(1).QuerySearchContainer(_WORKSPACE_ID, Data.Constants.DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME);
            _keywordSearchRepository.Received(1).CreateSearchContainerInRoot(_WORKSPACE_ID, Data.Constants.DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME);
        }
    }
}
