using System.Threading.Tasks;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Storage
{
	internal sealed class JobHistoryErrorRepository : IJobHistoryErrorRepository
	{
		private readonly ISourceServiceFactoryForAdmin _sourceServiceFactoryForAdmin;

		public JobHistoryErrorRepository(ISourceServiceFactoryForAdmin sourceServiceFactoryForAdmin)
		{
			_sourceServiceFactoryForAdmin = sourceServiceFactoryForAdmin;
		}

		public async Task<IJobHistoryError> CreateAsync(int workspaceArtifactId, CreateJobHistoryErrorDto createJobHistoryErrorDto)
		{
			IJobHistoryError jobHistoryError = await JobHistoryError.CreateAsync(_sourceServiceFactoryForAdmin, workspaceArtifactId, createJobHistoryErrorDto).ConfigureAwait(false);
			return jobHistoryError;
		}
	}
}