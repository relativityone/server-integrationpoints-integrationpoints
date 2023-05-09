using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Tagging
{
    [TestFixture, Category("Unit")]
    public class TagSavedSearchManagerTests : TestBase
    {
        private TagSavedSearchManager _instance;
        private ITagSavedSearch _tagSavedSearch;
        private ITagSavedSearchFolder _tagSavedSearchFolder;

        public override void SetUp()
        {
            _tagSavedSearch = Substitute.For<ITagSavedSearch>();
            _tagSavedSearchFolder = Substitute.For<ITagSavedSearchFolder>();

            _instance = new TagSavedSearchManager(_tagSavedSearch, _tagSavedSearchFolder);
        }

        [Test]
        public void ItShouldCreateSavedSearchForTagging()
        {
            int folderId = 919874;
            int workspaceId = 753626;
            var tagsContainer = new TagsContainer(new SourceJobDTO(), new SourceWorkspaceDTO());
            var importSettings = new DestinationConfiguration
            {
                CreateSavedSearchForTagging = true
            };

            _tagSavedSearchFolder.GetFolderId(workspaceId).Returns(folderId);

            // ACT
            _instance.CreateSavedSearchForTagging(workspaceId, importSettings, tagsContainer);

            // ASSERT
            _tagSavedSearchFolder.Received(1).GetFolderId(workspaceId);
            _tagSavedSearch.Received(1).CreateTagSavedSearch(workspaceId, tagsContainer, folderId);
        }

        [Test]
        public void ItShouldSkipCreatingSavedSearchForTaggingBasedOnSettings()
        {
            int folderId = 919874;
            int workspaceId = 753626;
            var tagsContainer = new TagsContainer(new SourceJobDTO(), new SourceWorkspaceDTO());
            var destinationConfiguration = new DestinationConfiguration
            {
                CreateSavedSearchForTagging = false
            };

            _tagSavedSearchFolder.GetFolderId(workspaceId).Returns(folderId);

            // ACT
            _instance.CreateSavedSearchForTagging(workspaceId, destinationConfiguration, tagsContainer);

            // ASSERT
            _tagSavedSearchFolder.Received(0).GetFolderId(workspaceId);
            _tagSavedSearch.Received(0).CreateTagSavedSearch(workspaceId, tagsContainer, folderId);
        }
    }
}
