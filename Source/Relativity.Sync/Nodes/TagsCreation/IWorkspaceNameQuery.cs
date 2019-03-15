using System.Threading.Tasks;

namespace Relativity.Sync.Nodes.TagsCreation
{
	internal interface IWorkspaceNameQuery
	{
		Task<string> GetWorkspaceNameAsync(int workspaceArtifactId);
	}
}