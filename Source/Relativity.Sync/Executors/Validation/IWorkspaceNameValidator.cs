using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
	internal interface IWorkspaceNameValidator
	{
		Task<bool> ValidateWorkspaceNameAsync(int workspaceArtifactId, CancellationToken token);
	}
}