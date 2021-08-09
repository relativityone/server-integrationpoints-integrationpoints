using System.Threading.Tasks;
using Relativity.Sync.Logging;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;

namespace Relativity.Sync.Extensions
{
    /// <summary>
    /// Extensions for <see cref="ISyncServiceManager"/>
    /// </summary>
    public static class SyncServiceManagerExtensions
    {
        /// <summary>
        /// Prepares SyncConfiguration for resuming paused job 
        /// </summary>
        public static Task PrepareSyncConfigurationForResumeAsync(this ISyncServiceManager serviceManager, int workspaceId,
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