using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
	internal interface IDocumentFieldRepository
	{
		Task<Dictionary<string, RelativityDataType>> GetRelativityDataTypesForFieldsByFieldName(int sourceWorkspaceArtifactId, ICollection<string> fieldNames);
	}
}