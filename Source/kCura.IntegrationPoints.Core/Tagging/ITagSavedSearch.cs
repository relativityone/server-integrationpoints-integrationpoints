namespace kCura.IntegrationPoints.Core.Tagging
{
	public interface ITagSavedSearch
	{
		void CreateTagSavedSearch(int workspaceArtifactId, TagsContainer tagsContainer, int savedSearchFolderId);
	}
}