namespace kCura.IntegrationPoints.Core.Tagging
{
    public interface ITagsCreator
    {
        TagsContainer CreateTags(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, int jobHistoryArtifactId, int? federatedInstanceArtifactId);
    }
}
