using System;
using System.Threading.Tasks;

namespace Relativity.Sync
{
    internal interface IProgressHandler
    {
        Task<IDisposable> AttachAsync(int workspaceId, Guid importJobId);
    }
}
