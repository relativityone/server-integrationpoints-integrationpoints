using System;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
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
        public static Task PrepareSyncConfigurationForResumeAsync(this IServicesMgr serviceManager, int workspaceId,
            int syncConfigurationId, IAPILog logger)
        {
            var rdo = new SyncConfigurationRdo
            {
                ArtifactId = syncConfigurationId,
            };

            ServiceFactoryForAdminFactory servicesManagerForAdminFactory = new ServiceFactoryForAdminFactory(serviceManager, logger);
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin = servicesManagerForAdminFactory.Create();

            return new RdoManager(logger, serviceFactoryForAdmin, new RdoGuidProvider())
                .SetValueAsync(workspaceId, rdo, x => x.Resuming, true);
        }

        /// <summary>
        /// Ensures that correct Sync Configuration object is configured on workspace and returns SyncConfigurationId for given JobHistoryId
        /// </summary>
        public static async Task<int?> TryGetResumedSyncConfigurationIdAsync(this IServicesMgr serviceManager, int workspaceId, int jobHistoryId, IAPILog logger)
        {
            ServiceFactoryForAdminFactory servicesManagerForAdminFactory = new ServiceFactoryForAdminFactory(serviceManager, logger);
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin = servicesManagerForAdminFactory.Create();
            IRdoManager rdoManager = new RdoManager(logger, serviceFactoryForAdmin, new RdoGuidProvider());

            await rdoManager.EnsureTypeExistsAsync<SyncConfigurationRdo>(workspaceId).ConfigureAwait(false);

            QueryRequest syncConfigurationId = new QueryRequest
            {
                ObjectType = new ObjectTypeRef { Guid = new Guid(SyncRdoGuids.SyncConfigurationGuid) },
                Condition = $"'JobHistoryId' == {jobHistoryId}"
            };

            using (IObjectManager objectManager = await serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                QueryResultSlim result = await objectManager.QuerySlimAsync(workspaceId, syncConfigurationId, 0, int.MaxValue)
                    .ConfigureAwait(false);

                if (result.Objects.Count == 1)
                {
                    return result.Objects.Single().ArtifactID;
                }

                if (result.Objects.Count > 1)
                {
                    logger.LogWarning(
                        "For JobHistory {jobHistory} has been found {count} Sync Configurations. System create new Sync Configuration instance",
                        jobHistoryId,
                        result.Objects.Count);
                }
            }

            return null;
        }
    }
}
