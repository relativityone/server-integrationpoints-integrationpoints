using System;
using System.Data;
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
using System.Linq.Expressions;

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
            SetRdoFields();
        }

        private void SetRdoFields()
        {
            // JobHistory
            SyncConfiguration.JobHistoryCompletedItemsField = RdoOptions.JobHistory.CompletedItemsCountGuid;
            SyncConfiguration.JobHistoryDestinationWorkspaceInformationField =
                RdoOptions.JobHistory.DestinationWorkspaceInformationGuid;
            SyncConfiguration.JobHistoryGuidFailedField = RdoOptions.JobHistory.FailedItemsCountGuid;
            SyncConfiguration.JobHistoryType = RdoOptions.JobHistory.JobHistoryTypeGuid;
            SyncConfiguration.JobHistoryGuidTotalField = RdoOptions.JobHistory.TotalItemsCountGuid;

            // JobHistoryError
            SyncConfiguration.JobHistoryErrorErrorMessages = RdoOptions.JobHistoryError.ErrorMessageGuid;
            SyncConfiguration.JobHistoryErrorErrorStatus = RdoOptions.JobHistoryError.ErrorStatusGuid;
            SyncConfiguration.JobHistoryErrorErrorType = RdoOptions.JobHistoryError.ErrorTypeGuid;
            SyncConfiguration.JobHistoryErrorItemLevelError = RdoOptions.JobHistoryError.ItemLevelErrorChoiceGuid;
            SyncConfiguration.JobHistoryErrorJobLevelError = RdoOptions.JobHistoryError.JobLevelErrorChoiceGuid;
            SyncConfiguration.JobHistoryErrorName = RdoOptions.JobHistoryError.NameGuid;
            SyncConfiguration.JobHistoryErrorSourceUniqueId = RdoOptions.JobHistoryError.SourceUniqueIdGuid;
            SyncConfiguration.JobHistoryErrorStackTrace = RdoOptions.JobHistoryError.StackTraceGuid;
            SyncConfiguration.JobHistoryErrorTimeStamp = RdoOptions.JobHistoryError.TimeStampGuid;
            SyncConfiguration.JobHistoryErrorType = RdoOptions.JobHistoryError.TypeGuid;
            
            // JobHistoryErrorStatus
            SyncConfiguration.JobHistoryErrorNewChoice = RdoOptions.JobHistoryErrorStatus.NewGuid;
            SyncConfiguration.JobHistoryErrorExpiredChoice = RdoOptions.JobHistoryErrorStatus.ExpiredGuid;
            SyncConfiguration.JobHistoryErrorInProgressChoice = RdoOptions.JobHistoryErrorStatus.InProgressGuid;
            SyncConfiguration.JobHistoryErrorRetriedChoice = RdoOptions.JobHistoryErrorStatus.RetriedGuid;
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
            using (var guidManager = ServicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
            {
                await Task.WhenAll(
                    ValidateJobHistoryAsync(guidManager),
                    ValidateJobHistoryErrorAsync(guidManager),
                    ValidateJobHistoryErrorStatusAsync(guidManager)
                    );
            }
        }

        private Task ValidateJobHistoryAsync(IArtifactGuidManager artifactGuidManager)
        {
            return Task.WhenAll(
                ValidatePropertyAsync(RdoOptions.JobHistory,artifactGuidManager, x => x.CompletedItemsCountGuid),
                ValidatePropertyAsync(RdoOptions.JobHistory,artifactGuidManager, x => x.FailedItemsCountGuid),
                ValidatePropertyAsync(RdoOptions.JobHistory,artifactGuidManager, x => x.TotalItemsCountGuid),
                ValidatePropertyAsync(RdoOptions.JobHistory,artifactGuidManager, x => x.JobHistoryTypeGuid),
                ValidatePropertyAsync(RdoOptions.JobHistory,artifactGuidManager, x => x.DestinationWorkspaceInformationGuid)
            );
        }
        
        private Task ValidateJobHistoryErrorAsync(IArtifactGuidManager artifactGuidManager)
        {
            return Task.WhenAll(
                ValidatePropertyAsync(RdoOptions.JobHistoryError,artifactGuidManager, x => x.TypeGuid),
                ValidatePropertyAsync(RdoOptions.JobHistoryError,artifactGuidManager, x => x.NameGuid),
                ValidatePropertyAsync(RdoOptions.JobHistoryError,artifactGuidManager, x => x.SourceUniqueIdGuid),
                ValidatePropertyAsync(RdoOptions.JobHistoryError,artifactGuidManager, x => x.ErrorMessageGuid),
                ValidatePropertyAsync(RdoOptions.JobHistoryError,artifactGuidManager, x => x.TimeStampGuid),
                ValidatePropertyAsync(RdoOptions.JobHistoryError,artifactGuidManager, x => x.ErrorTypeGuid),
                ValidatePropertyAsync(RdoOptions.JobHistoryError,artifactGuidManager, x => x.StackTraceGuid),
                ValidatePropertyAsync(RdoOptions.JobHistoryError,artifactGuidManager, x => x.ErrorStatusGuid),
                ValidatePropertyAsync(RdoOptions.JobHistoryError,artifactGuidManager, x => x.ItemLevelErrorChoiceGuid),
                ValidatePropertyAsync(RdoOptions.JobHistoryError,artifactGuidManager, x => x.JobLevelErrorChoiceGuid)
            );
        }
        
        private Task ValidateJobHistoryErrorStatusAsync(IArtifactGuidManager artifactGuidManager)
        {
            return Task.WhenAll(
                ValidatePropertyAsync(RdoOptions.JobHistoryErrorStatus,artifactGuidManager, x => x.NewGuid),
                ValidatePropertyAsync(RdoOptions.JobHistoryErrorStatus,artifactGuidManager, x => x.ExpiredGuid),
                ValidatePropertyAsync(RdoOptions.JobHistoryErrorStatus,artifactGuidManager, x => x.InProgressGuid),
                ValidatePropertyAsync(RdoOptions.JobHistoryErrorStatus,artifactGuidManager, x => x.RetriedGuid)
            );
        }

        private async Task ValidatePropertyAsync<TRdo>(TRdo rdo, IArtifactGuidManager guidManager, Expression<Func<TRdo, Guid>> expression)
        {
            MemberExpression memberExpression = expression.Body as MemberExpression ?? throw new InvalidExpressionException("Expression needs to be a member expression");

            Guid guid = expression.Compile().Invoke(rdo);
            if (!await guidManager.GuidExistsAsync(SyncContext.SourceWorkspaceId, guid))
            {
                throw new InvalidSyncConfigurationException(
                    $"Guid {guid.ToString()} for {typeof(TRdo).Name}.{memberExpression.Member.Name} does not exits");
            }
        }
        
        protected abstract Task ValidateAsync();
    }
}