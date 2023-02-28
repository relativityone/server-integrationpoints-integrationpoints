using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data.Statistics
{
    public interface INativeFileSizeStatistics : IDocumentStatistics
    {
        long GetTotalFileSize(IEnumerable<int> artifactIds, int workspaceArtifactId);
    }
}
