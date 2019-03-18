using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
	internal interface IWorkspaceNameQuery
	{
		Task<string> GetWorkspaceNameAsync(int workspaceArtifactId);
	}
}