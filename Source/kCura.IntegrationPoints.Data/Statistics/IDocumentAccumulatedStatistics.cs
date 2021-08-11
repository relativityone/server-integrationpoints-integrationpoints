using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Statistics
{
	public interface IDocumentAccumulatedStatistics
	{
		Task<DocumentsStatistics> GetNativesStatisticsForSavedSearchAsync(int workspaceId, int savedSearchId, bool calculateSize);
		Task<DocumentsStatistics> GetImagesStatisticsForSavedSearchAsync(int workspaceId, int savedSearchId, bool calculateSize);
		Task<DocumentsStatistics> GetImagesStatisticsForProductionAsync(int workspaceId, int productionId, bool calculateSize);
	}
}