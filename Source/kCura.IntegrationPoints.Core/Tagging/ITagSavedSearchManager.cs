using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Tagging
{
    public interface ITagSavedSearchManager
    {
        void CreateSavedSearchForTagging(int destinationWorkspaceArtifactId, ImportSettings importSettings, TagsContainer tagsContainer);
    }
}
