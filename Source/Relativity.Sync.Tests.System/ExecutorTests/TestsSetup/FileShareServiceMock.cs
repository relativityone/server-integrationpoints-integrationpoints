using System.Threading.Tasks;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.System.ExecutorTests.TestsSetup
{
    internal class FileShareServiceMock : IFileShareService
    {
        public FileShareServiceMock(string workspaceFileSharePath)
        {
            WorkspaceFileSharePath = workspaceFileSharePath;
        }

        public string WorkspaceFileSharePath { get; }

        public Task<string> GetWorkspaceFileShareLocationAsync(int workspaceId)
        {
            return Task.FromResult(WorkspaceFileSharePath);
        }
    }
}
