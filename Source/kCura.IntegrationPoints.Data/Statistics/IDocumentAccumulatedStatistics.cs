using System.Threading;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Statistics
{
	public interface IDocumentAccumulatedStatistics
	{
		Task<DocumentsStatistics> GetNativesStatisticsForSavedSearchAsync(int workspaceId, int savedSearchId, CancellationToken token = default(CancellationToken));
		Task<DocumentsStatistics> GetImagesStatisticsForSavedSearchAsync(int workspaceId, int savedSearchId, bool calculateSize, CancellationToken token = default(CancellationToken));
		Task<DocumentsStatistics> GetImagesStatisticsForProductionAsync(int workspaceId, int productionId, CancellationToken token = default(CancellationToken));
	}
}