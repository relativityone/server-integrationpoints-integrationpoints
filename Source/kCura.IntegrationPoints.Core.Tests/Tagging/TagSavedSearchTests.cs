using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Core.Tests.Tagging
{
    [TestFixture, Category("Unit")]
    public class TagSavedSearchTests : TestBase
    {
        private IKeywordSearchRepository _keywordSearchRepository;
        private TagSavedSearch _instance;

        public override void SetUp()
        {
            IRepositoryFactory repositoryFactory = Substitute.For<IRepositoryFactory>();
            IMultiObjectSavedSearchCondition multiObjectSavedSearchCondition = Substitute.For<IMultiObjectSavedSearchCondition>();
            IHelper helper = Substitute.For<IHelper>();

            _keywordSearchRepository = Substitute.For<IKeywordSearchRepository>();
            repositoryFactory.GetKeywordSearchRepository().Returns(_keywordSearchRepository);

            _instance = new TagSavedSearch(repositoryFactory, multiObjectSavedSearchCondition, helper);
        }

        [Test]
        public void ItShouldCreateSavedSearch()
        {
            int workspaceId = 424281;
            int savedSearchFolderId = 984452;
            SourceJobDTO sourceJobDto = new SourceJobDTO
            {
                Name = "name_444"
            };
            SourceWorkspaceDTO sourceWorkspaceDto = new SourceWorkspaceDTO();
            TagsContainer tagsContainer = new TagsContainer(sourceJobDto, sourceWorkspaceDto);

            // ACT
            _instance.CreateTagSavedSearch(workspaceId, tagsContainer, savedSearchFolderId);

            // ASSERT
            _keywordSearchRepository.Received(1).CreateSavedSearch(workspaceId, Arg.Is<KeywordSearch>(x => ValidateKeywordSearch(x, sourceJobDto.Name, savedSearchFolderId)));
        }

        [Test]
        public void ItShouldCreateKeywordSearchForTagging()
        {
            int workspaceId = 424281;
            int savedSearchFolderId = 984452;
            SourceJobDTO sourceJobDto = new SourceJobDTO
            {
                Name = "TwentyFourCharactersLongTwentyFourCharactersLongTwentyFourCharactersLong"
            };
            SourceWorkspaceDTO sourceWorkspaceDto = new SourceWorkspaceDTO();
            TagsContainer tagsContainer = new TagsContainer(sourceJobDto, sourceWorkspaceDto);

            var expectedName = "TwentyFourCharactersLo...gTwentyFourCharactersLong";

            // ACT
            _instance.CreateTagSavedSearch(workspaceId, tagsContainer, savedSearchFolderId);

            // ASSERT
            _keywordSearchRepository.Received(1).CreateSavedSearch(workspaceId, Arg.Is<KeywordSearch>(x => ValidateKeywordSearch(x, expectedName, savedSearchFolderId)));
        }

        private bool ValidateKeywordSearch(KeywordSearch keywordSearch, string name, int folderId)
        {
            return keywordSearch.ArtifactTypeID == (int) ArtifactType.Document
                    && keywordSearch.SearchContainer.ArtifactID == folderId
                    && keywordSearch.Name == name
                    && keywordSearch.Fields.Count == 3;
        }
    }
}
