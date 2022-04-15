using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
	internal interface IFolderPathRetriever
	{
		Task<IDictionary<int, string>> GetFolderPathsAsync(int workspaceArtifactId, ICollection<int> documentArtifactIds);
	}
}
