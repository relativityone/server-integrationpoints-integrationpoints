﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.ArtifactGuid;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Utils;
using System.Linq.Expressions;
using Relativity.Sync.RDOs.Framework;

namespace Relativity.Sync.SyncConfiguration
{
    internal abstract class SyncConfigurationRootBuilderBase : ISyncConfigurationRootBuilder
    {
        protected readonly ISyncServiceManager ServicesMgr;
        protected readonly RdoOptions RdoOptions;
        private readonly IRdoManager _rdoManager;
        protected readonly ISerializer Serializer;
        protected readonly ISyncContext SyncContext;

        public readonly SyncConfigurationRdo SyncConfiguration;

        protected SyncConfigurationRootBuilderBase(ISyncContext syncContext, ISyncServiceManager servicesMgr,
            RdoOptions rdoOptions, IRdoManager rdoManager, ISerializer serializer)
        {
            SyncContext = syncContext;
            ServicesMgr = servicesMgr;
            RdoOptions = rdoOptions;
            _rdoManager = rdoManager;
            Serializer = serializer;

            SyncConfiguration = new SyncConfigurationRdo 
            {
                CorrelationId = Guid.NewGuid().ToString(),
                DestinationWorkspaceArtifactId = syncContext.DestinationWorkspaceId,
                JobHistoryId =  syncContext.JobHistoryId,
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
            SyncConfiguration.JobHistoryErrorJobHistoryRelation = RdoOptions.JobHistoryError.JobHistoryRelationGuid;
            SyncConfiguration.JobHistoryErrorNewChoice = RdoOptions.JobHistoryError.NewStatusGuid;
            
            // DestinationWorkspace
            SyncConfiguration.DestinationWorkspaceType = RdoOptions.DestinationWorkspace.TypeGuid;
            SyncConfiguration.DestinationWorkspaceNameField = RdoOptions.DestinationWorkspace.NameGuid;
            SyncConfiguration.DestinationWorkspaceWorkspaceArtifactIdField = RdoOptions.DestinationWorkspace.DestinationWorkspaceArtifactIdGuid;
            SyncConfiguration.DestinationWorkspaceDestinationWorkspaceName = RdoOptions.DestinationWorkspace.DestinationWorkspaceNameGuid;
            SyncConfiguration.DestinationWorkspaceDestinationInstanceName = RdoOptions.DestinationWorkspace.DestinationInstanceNameGuid;
            SyncConfiguration.DestinationWorkspaceDestinationInstanceArtifactId = RdoOptions.DestinationWorkspace.DestinationInstanceArtifactIdGuid;
            SyncConfiguration.JobHistoryOnDocumentField = RdoOptions.DestinationWorkspace.JobHistoryOnDocumentGuid;
            SyncConfiguration.DestinationWorkspaceOnDocumentField = RdoOptions.DestinationWorkspace.DestinationWorkspaceOnDocument;
        }

        public void CorrelationId(string correlationId)
        {
	        SyncConfiguration.CorrelationId = correlationId;
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
	        List<(Guid Guid, string PropertyPath)> allValidationInfos = GetAllValidationInfos();
	        await ValidateGuidsAsync(allValidationInfos).ConfigureAwait(false);
            await ValidateAsync().ConfigureAwait(false);

            await _rdoManager.EnsureTypeExistsAsync<SyncConfigurationRdo>(SyncContext.SourceWorkspaceId).ConfigureAwait(false);
            await _rdoManager.EnsureTypeExistsAsync<SyncProgressRdo>(SyncContext.SourceWorkspaceId).ConfigureAwait(false);
            await _rdoManager.EnsureTypeExistsAsync<SyncBatchRdo>(SyncContext.SourceWorkspaceId).ConfigureAwait(false);

            await _rdoManager.CreateAsync(SyncContext.SourceWorkspaceId, SyncConfiguration).ConfigureAwait(false);

            return SyncConfiguration.ArtifactId;
        }
        
        private List<(Guid Guid, string PropertyPath)> GetAllValidationInfos()
        {
            return new List<(Guid Guid, string PropertyPath)>
            {
                // JobHistory
                GetValidationInfo(RdoOptions.JobHistory, x => x.CompletedItemsCountGuid),
                GetValidationInfo(RdoOptions.JobHistory, x => x.FailedItemsCountGuid),
                GetValidationInfo(RdoOptions.JobHistory, x => x.TotalItemsCountGuid),
                GetValidationInfo(RdoOptions.JobHistory, x => x.JobHistoryTypeGuid),
                GetValidationInfo(RdoOptions.JobHistory, x => x.DestinationWorkspaceInformationGuid),
                
                // JobHistoryError
                GetValidationInfo(RdoOptions.JobHistoryError, x => x.TypeGuid),
                GetValidationInfo(RdoOptions.JobHistoryError, x => x.NameGuid),
                GetValidationInfo(RdoOptions.JobHistoryError, x => x.SourceUniqueIdGuid),
                GetValidationInfo(RdoOptions.JobHistoryError, x => x.ErrorMessageGuid),
                GetValidationInfo(RdoOptions.JobHistoryError, x => x.TimeStampGuid),
                GetValidationInfo(RdoOptions.JobHistoryError, x => x.ErrorTypeGuid),
                GetValidationInfo(RdoOptions.JobHistoryError, x => x.StackTraceGuid),
                GetValidationInfo(RdoOptions.JobHistoryError, x => x.ErrorStatusGuid),
                GetValidationInfo(RdoOptions.JobHistoryError, x => x.JobHistoryRelationGuid),
                GetValidationInfo(RdoOptions.JobHistoryError, x => x.ItemLevelErrorChoiceGuid),
                GetValidationInfo(RdoOptions.JobHistoryError, x => x.JobLevelErrorChoiceGuid),
                GetValidationInfo(RdoOptions.JobHistoryError, x => x.NewStatusGuid),
                
                // DestinationWorkspace
                GetValidationInfo(RdoOptions.DestinationWorkspace, x => x.TypeGuid),
                GetValidationInfo(RdoOptions.DestinationWorkspace, x => x.NameGuid),
                GetValidationInfo(RdoOptions.DestinationWorkspace, x => x.DestinationWorkspaceNameGuid),
                GetValidationInfo(RdoOptions.DestinationWorkspace, x => x.DestinationWorkspaceArtifactIdGuid),
                GetValidationInfo(RdoOptions.DestinationWorkspace, x => x.DestinationInstanceNameGuid),
                GetValidationInfo(RdoOptions.DestinationWorkspace, x => x.DestinationInstanceArtifactIdGuid),
                GetValidationInfo(RdoOptions.DestinationWorkspace, x => x.JobHistoryOnDocumentGuid),
                GetValidationInfo(RdoOptions.DestinationWorkspace, x => x.DestinationWorkspaceOnDocument)
            };
        }
        
        private (Guid Guid, string PropertyPath) GetValidationInfo<TRdo>(TRdo rdo,
            Expression<Func<TRdo, Guid>> expression)
        {
            MemberExpression memberExpression = expression.Body as MemberExpression ??
                                                throw new InvalidExpressionException(
                                                    "Expression needs to be a member expression");

            Guid guid = expression.Compile().Invoke(rdo);
            string propertyPath = $"{typeof(TRdo).Name}.{memberExpression.Member.Name}";
            return (guid, propertyPath);
        }

        private async Task ValidateGuidsAsync(List<(Guid Guid, string PropertyPath)> validationInfos)
        {
            using (var guidManager = ServicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
            {
                List<GuidArtifactIDPair> guidArtifactIdPairs = await guidManager
                    .ReadMultipleArtifactIdsAsync(SyncContext.SourceWorkspaceId,
                        validationInfos.Select(x => x.Guid).ToList())
                    .ConfigureAwait(false);

                HashSet<Guid> existingGuids = new HashSet<Guid>(guidArtifactIdPairs.Select(x => x.Guid));

                string[] notExistingGuidErrors = validationInfos.Where(x => !existingGuids.Contains(x.Guid))
                    .Select(x => $"Guid {x.Guid.ToString()} for {x.PropertyPath} does not exits")
                    .ToArray();

                if (notExistingGuidErrors.Any())
                {
                    throw new InvalidSyncConfigurationException(string.Join(Environment.NewLine,
                        notExistingGuidErrors));
                }
            }
        }

        protected abstract Task ValidateAsync();
    }
}