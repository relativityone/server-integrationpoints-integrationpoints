using System.Threading.Tasks;
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
        public static Task PrepareSyncConfigurationForResumeAsync(this ISourceServiceFactoryForAdmin serviceManager, int workspaceId,
            int syncConfigurationId)
        {
            var rdo = new SyncConfigurationRdo
            {
                ArtifactId = syncConfigurationId,
            };

            return new RdoManager(new EmptyLogger(), serviceManager, new RdoGuidProvider())
                .SetValueAsync(workspaceId, rdo, x => x.Resuming, true);
        }
    }
}