using System;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
    internal interface ILoadFilePathService
    {
        Task<string> GetJobDirectoryPathAsync(int destinationWorkspaceId, Guid exportRunId);
    }
}
