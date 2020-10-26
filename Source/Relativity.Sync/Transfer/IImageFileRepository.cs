using System.Threading.Tasks;
using System.Collections.Generic;
using Relativity.Sync.Telemetry;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal interface IImageFileRepository
	{
		Task<IEnumerable<ImageFile>> QueryImagesForDocumentsAsync(int workspaceId, int[] documentIds, QueryImagesOptions options);

		Task<ImagesStatistics> CalculateImagesStatisticsAsync(int workspaceId, QueryRequest request, QueryImagesOptions options);
	}
}
