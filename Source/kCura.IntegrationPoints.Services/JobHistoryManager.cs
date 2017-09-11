using System;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Helpers;
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
		/// <param name="container"></param>
		internal JobHistoryManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory, IWindsorContainer container)
			: base(logger, permissionRepositoryFactory, container)
		{
		}

		public JobHistoryManager(ILog logger) : base(logger)
		{
		}

		public async Task<JobHistorySummaryModel> GetJobHistoryAsync(JobHistoryRequest request)
		{
			CheckPermissions(nameof(GetJobHistoryAsync), request.WorkspaceArtifactId,
				new[] {new PermissionModel(ObjectTypeGuids.JobHistory, ObjectTypes.JobHistory, ArtifactPermission.View)});
			try
			{
				using (IWindsorContainer container = GetDependenciesContainer(request.WorkspaceArtifactId))
				{
					var jobHistoryRepository = container.Resolve<IJobHistoryRepository>();
					return await Task.Run(() => jobHistoryRepository.GetJobHistory(request)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetJobHistoryAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public void Dispose()
		{
		}

		protected override Installer Installer => _installer ?? (_installer = new JobHistoryManagerInstaller());
	}
}