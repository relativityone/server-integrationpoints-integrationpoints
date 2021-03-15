using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal interface ISnapshotQueryRequestProvider
	{
		QueryRequest GetRequestForCurrentPipeline();
	}
}
