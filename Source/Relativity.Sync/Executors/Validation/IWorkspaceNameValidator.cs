using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors.Validation
{
	internal interface IWorkspaceNameValidator
	{
		Task<bool> ValidateWorkspaceNameAsync(IProxyFactory proxyFactory, int workspaceArtifactId, CancellationToken token);
	}
}