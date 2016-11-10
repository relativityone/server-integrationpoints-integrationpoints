using System.Threading.Tasks;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using kCura.IntegrationPoints.Services.Repositories;
using Relativity.API;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services
{
	/// <summary>
	///     This class is using direct sql because kepler does not provide the ability to aggregate data.
	/// </summary>
	public class JobHistoryManager : KeplerServiceBase, IJobHistoryManager
	{
		private readonly IJobHistoryRepository _jobHistoryRepository;

		/// <summary>
		///     For testing purposes only
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="permissionRepositoryFactory"></param>
		/// <param name="jobHistoryRepository"></param>
		internal JobHistoryManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory, IJobHistoryRepository jobHistoryRepository)
			: base(logger, permissionRepositoryFactory)
		{
			_jobHistoryRepository = jobHistoryRepository;
		}

		public JobHistoryManager(ILog logger) : base(logger)
		{
			_jobHistoryRepository = new JobHistoryRepository(logger);
		}

		public void Dispose()
		{
		}

		public async Task<JobHistorySummaryModel> GetJobHistoryAsync(JobHistoryRequest request)
		{
			return await Execute(() => _jobHistoryRepository.GetJobHistory(request), request.WorkspaceArtifactId).ConfigureAwait(false);
		}
	}
}