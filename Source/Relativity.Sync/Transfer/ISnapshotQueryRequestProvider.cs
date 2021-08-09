using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal interface ISnapshotQueryRequestProvider
	{
		Task<QueryRequest> GetRequestForCurrentPipelineAsync(CancellationToken token);
		
		Task<QueryRequest> GetRequestWithIdentifierOnlyForCurrentPipelineAsync(CancellationToken token);
	}
}
