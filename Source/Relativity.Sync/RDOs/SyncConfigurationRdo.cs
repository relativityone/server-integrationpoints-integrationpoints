using System;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.RDOs.Framework.Attributes;

namespace Relativity.Sync.RDOs
{
    [Rdo(SyncRdoGuids.SyncConfigurationGuid, "Sync Configuration")]
    internal sealed class SyncConfigurationRdo : IRdoType
    {
	    public int ArtifactId { get; set; }

        [RdoField(SyncRdoGuids.CorrelationIdGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public string CorrelationId { get; set; }

        [RdoField(SyncRdoGuids.ResumingGuid, RdoFieldType.YesNo)]
        public bool Resuming { get; set; }

        [RdoField(SyncRdoGuids.SyncStatisticsIdGuid, RdoFieldType.WholeNumber)]
        public int SyncStatisticsId { get; set; }

        [RdoField(SyncRdoGuids.RdoArtifactTypeIdGuid, RdoFieldType.WholeNumber)]
        public int RdoArtifactTypeId { get; set; }

        [RdoField(SyncRdoGuids.DestinationRdoArtifactTypeIdGuid, RdoFieldType.WholeNumber)]
        public int DestinationRdoArtifactTypeId { get; set; }

        [RdoEnumField(SyncRdoGuids.DataSourceTypeGuid)]
        public DataSourceType DataSourceType { get; set; }

        [RdoField(SyncRdoGuids.DataSourceArtifactIdGuid, RdoFieldType.WholeNumber)]
        public int DataSourceArtifactId { get; set; }

        [RdoField(SyncRdoGuids.DestinationWorkspaceArtifactIdGuid, RdoFieldType.WholeNumber)]
        public int DestinationWorkspaceArtifactId { get; set; }

        [RdoField(SyncRdoGuids.DataDestinationArtifactIdGuid, RdoFieldType.WholeNumber)]
        public int DataDestinationArtifactId { get; set; }

        [RdoEnumField(SyncRdoGuids.DataDestinationTypeGuid)]
        public DestinationLocationType DataDestinationType { get; set; }

        [RdoEnumField(SyncRdoGuids.DestinationFolderStructureBehaviorGuid)]
        public DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; set; }

        [RdoField(SyncRdoGuids.FolderPathSourceFieldNameGuid, RdoFieldType.FixedLengthText)]
        public string FolderPathSourceFieldName { get; set; }

        [RdoField(SyncRdoGuids.CreateSavedSearchInDestinationGuid, RdoFieldType.YesNo)]
        public bool CreateSavedSearchInDestination { get; set; }

        [RdoField(SyncRdoGuids.SavedSearchInDestinationArtifactIdGuid, RdoFieldType.WholeNumber)]
        public int SavedSearchInDestinationArtifactId { get; set; }

        [RdoEnumField(SyncRdoGuids.ImportOverwriteModeGuid)]
        public ImportOverwriteMode ImportOverwriteMode { get; set; }

        [RdoEnumField(SyncRdoGuids.FieldOverlayBehaviorGuid)]
        public FieldOverlayBehavior FieldOverlayBehavior { get; set; }

        [RdoField(SyncRdoGuids.FieldMappingsGuid, RdoFieldType.LongText)]
        public string FieldsMapping { get; set; }

        [RdoField(SyncRdoGuids.MoveExistingDocumentsGuid, RdoFieldType.YesNo)]
        public bool MoveExistingDocuments { get; set; }

        [RdoEnumField(SyncRdoGuids.NativesBehaviorGuid)]
        public ImportNativeFileCopyMode NativesBehavior { get; set; } = ImportNativeFileCopyMode.DoNotImportNativeFiles;

        [RdoField(SyncRdoGuids.ImageImportGuid, RdoFieldType.YesNo)]
        public bool ImageImport { get; set; }

        [RdoField(SyncRdoGuids.IncludeOriginalImagesGuid, RdoFieldType.YesNo)]
        public bool IncludeOriginalImages { get; set; }

        [RdoField(SyncRdoGuids.ProductionImagePrecedenceGuid, RdoFieldType.LongText)]
        public string ProductionImagePrecedence { get; set; }

        [RdoEnumField(SyncRdoGuids.ImageFileCopyModeGuid)]
        public ImportImageFileCopyMode ImageFileCopyMode { get; set; } = ImportImageFileCopyMode.DoNotImportImageFiles;

        [RdoField(SyncRdoGuids.EmailNotificationRecipientsGuid, RdoFieldType.LongText)]
        public string EmailNotificationRecipients { get; set; }

        [RdoField(SyncRdoGuids.JobHistoryIdGuid, RdoFieldType.WholeNumber, required: false)]
        public int JobHistoryId { get; set; }
        
        [RdoField(SyncRdoGuids.JobHistoryToRetryIdGuid, RdoFieldType.WholeNumber, required: false)]
        public int? JobHistoryToRetryId { get; set; }

        [RdoField(SyncRdoGuids.SnapshotIdGuid, RdoFieldType.FixedLengthText)]
        public Guid? SnapshotId { get; set; }

        [RdoField(SyncRdoGuids.ObjectLinkingSnapshotGuid, RdoFieldType.FixedLengthText)]
        public Guid? ObjectLinkingSnapshotId { get; set; }

        [RdoField(SyncRdoGuids.SnapshotRecordsCountGuid, RdoFieldType.WholeNumber)]
        public int SnapshotRecordsCount { get; set; }

        [RdoField(SyncRdoGuids.SourceJobTagArtifactIdGuid, RdoFieldType.WholeNumber)]
        public int SourceJobTagArtifactId { get; set; }

        [RdoField(SyncRdoGuids.SourceJobTagNameGuid, RdoFieldType.FixedLengthText)]
        public string SourceJobTagName { get; set; }

        [RdoField(SyncRdoGuids.SourceWorkspaceTagArtifactIdGuid, RdoFieldType.WholeNumber)]
        public int SourceWorkspaceTagArtifactId { get; set; }

        [RdoField(SyncRdoGuids.SourceWorkspaceTagNameGuid, RdoFieldType.FixedLengthText)]
        public string SourceWorkspaceTagName { get; set; }

        [RdoField(SyncRdoGuids.DestinationWorkspaceTagArtifactIdGuid, RdoFieldType.WholeNumber)]
        public int DestinationWorkspaceTagArtifactId { get; set; }
        
        // JobHistory configuration
        [RdoField(SyncRdoGuids.JobHistoryTypeGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryType { get; set; }

        [RdoField(SyncRdoGuids.JobHistoryCompletedItemsFieldGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryCompletedItemsField { get; set; }
        
        [RdoField(SyncRdoGuids.JobHistoryFailedItemsFieldGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryGuidFailedField { get; set; }
        
        [RdoField(SyncRdoGuids.JobHistoryTotalItemsFieldGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryGuidTotalField { get; set; }
        
        [RdoField(SyncRdoGuids.JobHistoryDestinationWorkspaceInformationGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryDestinationWorkspaceInformationField { get; set; }

        // JobHistoryError configuration

        [RdoField(SyncRdoGuids.JobHistoryErrorTypeGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryErrorType { get; set; }
        
        [RdoField(SyncRdoGuids.JobHistoryErrorErrorMessagesGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryErrorErrorMessages { get; set; }
        
        [RdoField(SyncRdoGuids.JobHistoryErrorErrorStatusGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryErrorErrorStatus { get; set; }
        
        [RdoField(SyncRdoGuids.JobHistoryErrorErrorTypeGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryErrorErrorType { get; set; }
        
        [RdoField(SyncRdoGuids.JobHistoryErrorNameGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryErrorName { get; set; }
        
        [RdoField(SyncRdoGuids.JobHistoryErrorSourceUniqueIdGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryErrorSourceUniqueId { get; set; }
        
        [RdoField(SyncRdoGuids.JobHistoryErrorStackTraceGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryErrorStackTrace { get; set; }
        
        [RdoField(SyncRdoGuids.JobHistoryErrorTimeStampGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryErrorTimeStamp { get; set; }
        
        [RdoField(SyncRdoGuids.JobHistoryErrorItemLevelErrorGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryErrorItemLevelError { get; set; }
        
        [RdoField(SyncRdoGuids.JobHistoryErrorJobLevelErrorGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryErrorJobLevelError { get; set; }
        
        [RdoField(SyncRdoGuids.JobHistoryErrorJobHistoryRelationGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryErrorJobHistoryRelation { get; set; }

        [RdoField(SyncRdoGuids.JobHistoryErrorNewChoiceGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryErrorNewChoice { get; set; }
        
        [RdoField(SyncRdoGuids.JobHistoryErrorExpiredChoiceGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryErrorExpiredChoice { get; set; }
        
        [RdoField(SyncRdoGuids.JobHistoryErrorInProgressChoiceGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryErrorInProgressChoice { get; set; }
        
        [RdoField(SyncRdoGuids.JobHistoryErrorRetriedChoiceGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryErrorRetriedChoice { get; set; }
        
        // DestinationWorkspace RDO configuration
        [RdoField(SyncRdoGuids.DestinationWorkspaceTypeGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid DestinationWorkspaceType { get; set; }
        
        [RdoField(SyncRdoGuids.DestinationWorkspaceNameFieldGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid DestinationWorkspaceNameField { get; set; }

        [RdoField(SyncRdoGuids.DestinationWorkspaceDestinationWorkspaceArtifactIdGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid DestinationWorkspaceWorkspaceArtifactIdField { get; set; }
        
        [RdoField(SyncRdoGuids.DestinationWorkspaceDestinationWorkspaceNameGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid DestinationWorkspaceDestinationWorkspaceName { get; set; }
        
        [RdoField(SyncRdoGuids.DestinationWorkspaceDestinationInstanceNameGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid DestinationWorkspaceDestinationInstanceName { get; set; }
        
        [RdoField(SyncRdoGuids.DestinationWorkspaceDestinationInstanceArtifactIdGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid DestinationWorkspaceDestinationInstanceArtifactId { get; set; }
        
        [RdoField(SyncRdoGuids.JobHistoryOnDocumentFieldGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid JobHistoryOnDocumentField { get; set; }

        [RdoField(SyncRdoGuids.DestinationWorkspaceOnDocumentFieldGuid, RdoFieldType.FixedLengthText,
            fixedTextLength: 36)]
        public Guid DestinationWorkspaceOnDocumentField { get; set; }

        [RdoField(SyncRdoGuids.ExecutingApplicationGuid, RdoFieldType.FixedLengthText)]
        public string ExecutingApplication { get; set; }
        
        [RdoField(SyncRdoGuids.ExecutingApplicationVersionGuid, RdoFieldType.FixedLengthText, fixedTextLength: 10)]
        public string ExecutingApplicationVersion { get; set; }
    }
}