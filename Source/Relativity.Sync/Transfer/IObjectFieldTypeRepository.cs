using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
    internal interface IObjectFieldTypeRepository
    {
        Task<IDictionary<string, RelativityDataType>> GetRelativityDataTypesForFieldsByFieldNameAsync(int sourceWorkspaceArtifactId, int sourceRdoArtifactId, ICollection<string> fieldNames, CancellationToken token);
    }
}
