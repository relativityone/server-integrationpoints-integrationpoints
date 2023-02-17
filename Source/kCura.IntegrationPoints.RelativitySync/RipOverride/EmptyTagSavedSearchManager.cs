using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.RelativitySync.RipOverride
{
    // This way we can disable creation of saved search in destination workspace.
    internal sealed class EmptyTagSavedSearchManager : ITagSavedSearchManager
    {
        public void CreateSavedSearchForTagging(int destinationWorkspaceArtifactId, ImportSettings importSettings, TagsContainer tagsContainer)
        {
            // method intentionally left empty
        }
    }
}
