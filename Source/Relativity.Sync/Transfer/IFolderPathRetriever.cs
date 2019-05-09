using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
	internal interface IFolderPathRetriever
	{
		Task<IEnumerable<string>> GetFolderPathsAsync(IEnumerable<int> documentArtifactIds);
		Task<string> GetFolderPathAsync(int documentArtifactId);
	}
}
