using System.Threading.Tasks;
using kCura.IntegrationPoints.Services.Installers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using kCura.IntegrationPoints.Services.Repositories;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services
{
	public class JobHistoryManager : KeplerServiceBase, IJobHistoryManager
	{
		private Installer _installer;

		/// <summary>
		///     For testing purposes only
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="permissionRepositoryFactory"></param>
		internal JobHistoryManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory)
			: base(logger, permissionRepositoryFactory)
		{
		}

		public JobHistoryManager(ILog logger) : base(logger)
		{
		}

		public async Task<JobHistorySummaryModel> GetJobHistoryAsync(JobHistoryRequest request)
		{
			return await Execute((IJobHistoryRepository jobHistoryRepository) => jobHistoryRepository.GetJobHistory(request), request.WorkspaceArtifactId).ConfigureAwait(false);
		}

		public void Dispose()
		{
		}

		protected override Installer Installer => _installer ?? (_installer = new JobHistoryManagerInstaller());
	}
}