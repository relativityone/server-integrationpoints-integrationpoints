using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
	internal interface IJobHistoryNameQuery
	{
		Task<string> GetJobNameAsync(int jobHistoryArtifactId, CancellationToken token);
	}
}