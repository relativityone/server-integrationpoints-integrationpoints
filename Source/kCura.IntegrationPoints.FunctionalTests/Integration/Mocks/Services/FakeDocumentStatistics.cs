using kCura.IntegrationPoints.Data.Statistics;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    class FakeDocumentStatistics : 
        IDocumentTotalStatistics, INativeTotalStatistics, IImageTotalStatistics, IImageFileSizeStatistics, INativeFileSizeStatistics
    {
        public long ForFolder(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
        {
            return 10L;
        }

        public long ForProduction(int workspaceArtifactId, int productionSetId)
        {
            return 10L;
        }

        public long ForSavedSearch(int workspaceArtifactId, int savedSearchId)
        {
            return 10L;
        }
    }
}
