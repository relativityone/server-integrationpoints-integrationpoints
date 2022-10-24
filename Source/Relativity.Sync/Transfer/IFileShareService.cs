using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
    internal interface IFileShareService
    {
        Task<string> GetWorkspaceFileShareLocationAsync(int workspaceId);
    }
}
