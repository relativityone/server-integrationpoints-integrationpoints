using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
	internal interface IRelativitySourceJobTagRepository
	{
		Task<RelativitySourceJobTag> ReadAsync(int sourceJobArtifactTypeId, int sourceCaseTagArtifactId, int sourceJobArtifactId, CancellationToken token);
		Task<RelativitySourceJobTag> CreateAsync(int sourceJobArtifactTypeId, RelativitySourceJobTag sourceJobTag, CancellationToken token);
		Task<RelativitySourceJobTag> UpdateAsync(int sourceJobArtifactTypeId, RelativitySourceJobTag sourceJobTag, CancellationToken token);
	}
}