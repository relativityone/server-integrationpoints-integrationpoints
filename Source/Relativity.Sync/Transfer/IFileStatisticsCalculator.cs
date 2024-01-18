using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Transfer
{
    internal interface IFileStatisticsCalculator
    {
        Task<long> CalculateNativesTotalSizeAsync(int workspaceId, QueryRequest request, CompositeCancellationToken token);

        Task<ImagesStatistics> CalculateImagesStatisticsAsync(int workspaceId, QueryRequest request, QueryImagesOptions options, CompositeCancellationToken token);
    }
}
