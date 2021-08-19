using System.Threading.Tasks;
using System.Collections.Generic;

namespace Relativity.Sync.Transfer
{
	internal interface IImageFileRepository
	{
		Task<IEnumerable<ImageFile>> QueryImagesForDocumentsAsync(int workspaceId, int[] documentIds,
			QueryImagesOptions options);
	}
}