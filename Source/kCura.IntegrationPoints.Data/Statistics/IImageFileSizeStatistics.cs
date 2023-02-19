using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data.Statistics
{
    public interface IImageFileSizeStatistics : IDocumentStatistics
    {
        long GetTotalFileSize(IList<int> artifactIds, int workspaceArtifactId);
        long GetTotalFileSize(int productionSetId, int workspaceArtifactId);
    }
}
