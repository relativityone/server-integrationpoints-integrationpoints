using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
	internal interface IRelativitySourceJobTagRepository
	{
		Task<RelativitySourceJobTag> CreateAsync(int sourceJobArtifactTypeId, int destinationWorkspaceArtifactId, RelativitySourceJobTag sourceJobTag, CancellationToken token);
	}
}