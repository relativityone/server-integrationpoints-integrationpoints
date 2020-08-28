using Relativity.Services.Objects.DataContracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
	internal interface IImageFileRepository
	{
		Task<IEnumerable<ImageFile>> QueryImagesforDocumentsAsync(int workspaceId, IList<int> documentIds, QueryImagesOptions options);

		Task<long> CalculateImagesTotalSizeAsync(int workspaceId, QueryRequest request, QueryImagesOptions options);
	}
}
