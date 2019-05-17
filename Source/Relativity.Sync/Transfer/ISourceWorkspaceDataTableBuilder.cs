using System.Data;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal interface ISourceWorkspaceDataTableBuilder
	{
		Task<DataTable> BuildAsync(RelativityObjectSlim[] batch);
	}
}
