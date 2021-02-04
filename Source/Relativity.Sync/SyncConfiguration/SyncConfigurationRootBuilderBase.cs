using System;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Utils;

#pragma warning disable 1591

namespace Relativity.Sync.SyncConfiguration
{
    internal abstract class SyncConfigurationRootBuilderBase : ISyncConfigurationRootBuilder
    {
        protected readonly ISyncServiceManager ServicesMgr;
        protected readonly RdoOptions RdoOptions;
        protected readonly ISerializer Serializer;
        protected readonly ISyncContext SyncContext;

        public readonly SyncConfigurationRdo SyncConfiguration;

        protected SyncConfigurationRootBuilderBase(ISyncContext syncContext, ISyncServiceManager servicesMgr,
            RdoOptions rdoOptions, ISerializer serializer)
        {
            SyncContext = syncContext;
            ServicesMgr = servicesMgr;
            RdoOptions = rdoOptions;
            Serializer = serializer;
            
            SyncConfiguration = new SyncConfigurationRdo
            {
                DestinationWorkspaceArtifactId = SyncContext.DestinationWorkspaceId,
                ImportOverwriteMode = ImportOverwriteMode.AppendOnly.GetDescription(),
                FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings.GetDescription()
            };
        }

        public void OverwriteMode(OverwriteOptions options)
        {
            SyncConfiguration.ImportOverwriteMode = options.OverwriteMode.ToString();
            SyncConfiguration.FieldOverlayBehavior = options.FieldsOverlayBehavior.GetDescription();
        }

        public void EmailNotifications(EmailNotificationsOptions options)
        {
            SyncConfiguration.EmailNotificationRecipients = string.Join(
                ";", options.Emails.Select(x => x.Trim()));
        }

        public void CreateSavedSearch(CreateSavedSearchOptions options)
        {
            SyncConfiguration.CreateSavedSearchInDestination = options.CreateSavedSearchInDestination;
        }

        public void IsRetry(RetryOptions options)
        {
            SyncConfiguration.JobHistoryToRetryId = options.JobToRetry;
        }

        public async Task<int> SaveAsync()
        {
            await ValidateRdosAsync().ConfigureAwait(false);
            await ValidateAsync().ConfigureAwait(false);

            int parentObjectTypeId = await GetParentObjectTypeAsync(SyncContext.SourceWorkspaceId, SyncContext.ParentObjectId); 
            
            await SyncConfigurationRdo
                .EnsureTypeExists(SyncContext.SourceWorkspaceId, parentObjectTypeId, ServicesMgr)
                .ConfigureAwait(false);

            return await SyncConfiguration
                .SaveAsync(SyncContext.SourceWorkspaceId, SyncContext.ParentObjectId, ServicesMgr)
                .ConfigureAwait(false);
        }

        private async Task<int> GetParentObjectTypeAsync(int workspaceId, int parentObjectId)
        {
            using (IObjectManager objectManager = ServicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
            {
                ReadRequest request = new ReadRequest()
                {
                    Object = new RelativityObjectRef()
                    {
                        ArtifactID = parentObjectId
                    }
                };

                ReadResult response = await objectManager.ReadAsync(workspaceId, request).ConfigureAwait(false);
                return response.ObjectType.ArtifactTypeID;
            }
        }

        private async Task<int> GetJobHistoryArtifactIdAsync(int workspaceId, Guid jobHistoryGuid)
        {
            using (var guidManager = ServicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
            {
                return await guidManager.ReadSingleArtifactIdAsync(workspaceId, jobHistoryGuid).ConfigureAwait(false);
            }
        }

        private async Task ValidateRdosAsync()
        {
            async Task ThrowIfDoesNotExist(IArtifactGuidManager guidManager, Guid guid, string fieldPath)
            {
                if (!await guidManager.GuidExistsAsync(SyncContext.SourceWorkspaceId, guid))
                {
                    throw new InvalidSyncConfigurationException(
                        $"Guid {guid.ToString()} for {fieldPath} does not exits");
                }
            }

            using (var guidManager = ServicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
            {
                await ThrowIfDoesNotExist(guidManager, RdoOptions.JobHistory.CompletedItemsCountGuid,
                    $"{nameof(JobHistoryOptions)}.{nameof(JobHistoryOptions.CompletedItemsCountGuid)}");
                
                await ThrowIfDoesNotExist(guidManager, RdoOptions.JobHistory.FailedItemsCountGuid,
                    $"{nameof(JobHistoryOptions)}.{nameof(JobHistoryOptions.FailedItemsCountGuid)}");
                
                await ThrowIfDoesNotExist(guidManager, RdoOptions.JobHistory.JobHistoryTypeGuid,
                    $"{nameof(JobHistoryOptions)}.{nameof(JobHistoryOptions.JobHistoryTypeGuid)}");
                
                await ThrowIfDoesNotExist(guidManager, RdoOptions.JobHistory.TotalItemsCountGuid,
                    $"{nameof(JobHistoryOptions)}.{nameof(JobHistoryOptions.TotalItemsCountGuid)}");
            }
        }
        
        

        protected abstract Task ValidateAsync();
    }
}