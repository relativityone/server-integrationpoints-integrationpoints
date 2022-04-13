using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;

namespace Relativity.Sync.Extensions
{
    /// <summary>
    /// Extensions for <see cref="ISourceServiceFactoryForAdmin"/>
    /// </summary>
    public static class SourceServiceFactoryForAdminExtensions
    {
        /// <summary>
        /// Prepares SyncConfiguration for resuming paused job 
        /// </summary>
        public static Task PrepareSyncConfigurationForResumeAsync(this ISyncServiceManager serviceManager, int workspaceId,
            int syncConfigurationId, IAPILog logger)
        {
            var rdo = new SyncConfigurationRdo
            {
                ArtifactId = syncConfigurationId,
            };

            ServiceFactoryForAdminFactory servicesManagerForAdminFactory = new ServiceFactoryForAdminFactory(serviceManager, logger);
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin = servicesManagerForAdminFactory.Create();

            return new RdoManager(new EmptyLogger(), serviceFactoryForAdmin, new RdoGuidProvider())
                .SetValueAsync(workspaceId, rdo, x => x.Resuming, true);
        }
    }
}
