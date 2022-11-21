using System;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.IntegrationPoints.Services.Installers;
using Relativity.IntegrationPoints.Services.Repositories;
using Relativity.Logging;

namespace Relativity.IntegrationPoints.Services
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
            CheckPermissions(
                nameof(GetJobHistoryAsync), 
                request.WorkspaceArtifactId,
                new[]
                {
                    new PermissionModel(ObjectTypeGuids.JobHistoryGuid, ObjectTypes.JobHistory, ArtifactPermission.View)
                });
            try
            {
                using (IWindsorContainer container = GetDependenciesContainer(request.WorkspaceArtifactId))
                {
                    var jobHistoryRepository = container.Resolve<IJobHistoryAccessor>();
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