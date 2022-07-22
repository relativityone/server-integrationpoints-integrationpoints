using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
    internal interface IWorkspaceNameQuery
    {
        Task<string> GetWorkspaceNameAsync(IProxyFactory proxyFactory, int workspaceArtifactId, CancellationToken token);
    }
}
