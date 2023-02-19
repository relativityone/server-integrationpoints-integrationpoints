namespace kCura.IntegrationPoints.Core.Tagging
{
    public interface ITagSavedSearch
    {
        int CreateTagSavedSearch(int workspaceArtifactId, TagsContainer tagsContainer, int savedSearchFolderId);
    }
}
