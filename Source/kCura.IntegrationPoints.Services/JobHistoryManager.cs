using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
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
			CheckJobHistoryPermissions(request.WorkspaceArtifactId);
			using (var container = GetDependenciesContainer(request.WorkspaceArtifactId))
			{
				IJobHistoryRepository jobHistoryRepository = container.Resolve<IJobHistoryRepository>();
				return await Task.Run(() => jobHistoryRepository.GetJobHistory(request));
			}
		}

		private void CheckJobHistoryPermissions(int workspaceId)
		{
			SafePermissionCheck(() =>
			{
				var permissionRepository = GetPermissionRepository(workspaceId);
				bool hasAccesToWorkspace = permissionRepository.UserHasPermissionToAccessWorkspace();
				bool hasAccesToViewJobHistory = permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.View);
				if (hasAccesToWorkspace && hasAccesToViewJobHistory)
				{
					return;
				}
				IList<string> missingPermissions = new List<string>();
				if (!hasAccesToWorkspace)
				{
					missingPermissions.Add("Workspace");
				}
				if (!hasAccesToViewJobHistory)
				{
					missingPermissions.Add("JobHistory - View");
				}
				LogAndThrowInsufficientPermissionException(nameof(GetJobHistoryAsync), missingPermissions);
			});
		}

		public void Dispose()
		{
		}

		protected override Installer Installer => _installer ?? (_installer = new JobHistoryManagerInstaller());
	}
}