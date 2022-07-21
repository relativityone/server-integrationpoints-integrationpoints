using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
    internal interface INativeFileRepository
    {
        Task<IEnumerable<INativeFile>> QueryAsync(int workspaceId, ICollection<int> documentIds);
    }
}
