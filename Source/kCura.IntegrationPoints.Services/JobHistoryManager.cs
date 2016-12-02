using System.Threading.Tasks;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Services.Installers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using kCura.IntegrationPoints.Services.Repositories;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services
{
	/// <summary>
	///     This class is using direct sql because kepler does not provide the ability to aggregate data.
	/// </summary>
	public class JobHistoryManager : KeplerServiceBase, IJobHistoryManager
	{
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
			return await Execute(() =>
			{
				using (var container = GetDependenciesContainerAsync())
				{
					var jobHistoryRepository = container.Resolve<IJobHistoryRepository>();
					return jobHistoryRepository.GetJobHistory(request);
				}
			}, request.WorkspaceArtifactId).ConfigureAwait(false);
		}

		private IWindsorContainer GetDependenciesContainerAsync()
		{
			IWindsorContainer container = new WindsorContainer();
			JobHistoryManagerInstaller installer = new JobHistoryManagerInstaller();
			installer.Install(container, new DefaultConfigurationStore());
			return container;
		}

		public void Dispose()
		{
		}
	}
}