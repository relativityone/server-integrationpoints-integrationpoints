using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
	internal interface IDocumentFieldRepository
	{
		Task<Dictionary<string, RelativityDataType>> GetRelativityDataTypesForFieldsByFieldNameAsync(int sourceWorkspaceArtifactId, ICollection<string> fieldNames, CancellationToken token);
	}
}