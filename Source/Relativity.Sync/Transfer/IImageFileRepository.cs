using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
    internal interface IImageFileRepository
    {
        Task<IEnumerable<ImageFile>> QueryImagesForDocumentsAsync(
            int workspaceId,
            int[] documentIds, QueryImagesOptions options);
    }
}
