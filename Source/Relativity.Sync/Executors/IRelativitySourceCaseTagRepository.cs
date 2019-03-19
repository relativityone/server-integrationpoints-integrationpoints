using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
	internal interface IRelativitySourceCaseTagRepository
	{
		Task<RelativitySourceCaseTag> CreateAsync(int sourceWorkspaceArtifactTypeId, RelativitySourceCaseTag sourceCaseTag, CancellationToken token);
		Task<RelativitySourceCaseTag> ReadAsync(int sourceWorkspaceArtifactTypeId, int sourceWorkspaceArtifactId, string sourceInstanceName, CancellationToken token);

		Task<RelativitySourceCaseTag> UpdateAsync(int sourceWorkspaceArtifactTypeId, RelativitySourceCaseTag sourceCaseTag, CancellationToken token);
	}
}
