using System;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
	internal interface IJobHistoryNameQuery
	{
		Task<string> GetJobNameAsync(Guid jobHistoryGuid, int jobHistoryArtifactId, int sourceWorkspaceArtifactId, CancellationToken token);
	}
}