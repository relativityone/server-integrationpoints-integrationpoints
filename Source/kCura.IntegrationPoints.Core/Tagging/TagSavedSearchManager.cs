using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Tagging
{
    public class TagSavedSearchManager : ITagSavedSearchManager
    {
        private readonly ITagSavedSearch _tagSavedSearch;
        private readonly ITagSavedSearchFolder _tagSavedSearchFolder;

        public TagSavedSearchManager(ITagSavedSearch tagSavedSearch, ITagSavedSearchFolder tagSavedSearchFolder)
        {
            _tagSavedSearch = tagSavedSearch;
            _tagSavedSearchFolder = tagSavedSearchFolder;
        }

        public void CreateSavedSearchForTagging(int destinationWorkspaceArtifactId, ImportSettings importSettings, TagsContainer tagsContainer)
        {
            CreateSavedSearchForTagging(destinationWorkspaceArtifactId, importSettings.CreateSavedSearchForTagging, tagsContainer);
        }

        public int CreateSavedSearchForTagging(int destinationWorkspaceArtifactId, bool createSavedSearch, TagsContainer tagsContainer)
        {
            if (createSavedSearch)
            {
                int folderId = _tagSavedSearchFolder.GetFolderId(destinationWorkspaceArtifactId);
                return _tagSavedSearch.CreateTagSavedSearch(destinationWorkspaceArtifactId, tagsContainer, folderId);
            }

            return 0;
        }
    }
}
