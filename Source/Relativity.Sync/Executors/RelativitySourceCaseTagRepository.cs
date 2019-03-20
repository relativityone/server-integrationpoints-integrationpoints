using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
	public class RelativitySourceCaseTagRepository : IRelativitySourceCaseTagRepository
	{
		public async Task<RelativitySourceCaseTag> CreateAsync(int sourceWorkspaceArtifactTypeId, RelativitySourceCaseTag sourceCaseTag, CancellationToken token)
		{
			throw new System.NotImplementedException();
		}

		public async Task<RelativitySourceCaseTag> ReadAsync(int sourceWorkspaceArtifactTypeId, int sourceWorkspaceArtifactId, string sourceInstanceName, CancellationToken token)
		{
			throw new System.NotImplementedException();
		}

		public async Task<RelativitySourceCaseTag> UpdateAsync(int sourceWorkspaceArtifactTypeId, RelativitySourceCaseTag sourceCaseTag, CancellationToken token)
		{
			throw new System.NotImplementedException();
		}
	}
}