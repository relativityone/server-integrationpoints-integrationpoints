using System.Threading.Tasks;

namespace Relativity.Sync.Executors.TagsCreation
{
	internal interface IWorkspaceNameQuery
	{
		Task<string> GetWorkspaceNameAsync(int workspaceArtifactId);
	}
}