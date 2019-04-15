namespace Relativity.Sync.Executors
{
	internal interface ITagSavedSearch
	{
		int CreateTagSavedSearch(int workspaceArtifactId, TagsContainer tagsContainer, int savedSearchFolderId);
	}
}