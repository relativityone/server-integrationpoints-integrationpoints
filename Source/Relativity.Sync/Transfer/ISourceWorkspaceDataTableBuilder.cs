using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Transfer
{
	internal interface ISourceWorkspaceDataTableBuilder
	{
		Task<DataTable> BuildAsync(int sourceWorkspaceArtifactId, IEnumerable<FieldMap> fieldMaps, RelativityObjectSlim[] batch);
	}
}