using System.Data;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal interface IBatchDataTableBuilder
	{
		Task<DataTable> BuildAsync(RelativityObjectSlim[] batch);
	}
}
