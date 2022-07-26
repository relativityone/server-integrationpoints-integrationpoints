using System;
using System.Collections.Generic;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Configuration
{
    internal interface IValidationConfiguration : IConfiguration
    {
        int SourceWorkspaceArtifactId { get; }

        int DestinationWorkspaceArtifactId { get; }

        int SavedSearchArtifactId { get; }

        int DestinationFolderArtifactId { get; }
        
        int RdoArtifactTypeId { get; }

        int DestinationRdoArtifactTypeId { get; }

        Guid JobHistoryObjectTypeGuid { get; }

        ImportOverwriteMode ImportOverwriteMode { get; }

        FieldOverlayBehavior FieldOverlayBehavior { get; }

        DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; }

        ImportNativeFileCopyMode ImportNativeFileCopyMode { get; }

        ImportImageFileCopyMode ImportImageFileCopyMode { get; }
        
        int? JobHistoryToRetryId { get; }

        string GetJobName();

        string GetNotificationEmails();

        IList<FieldMap> GetFieldMappings();

        string GetFolderPathSourceFieldName();
        
        bool Resuming { get; }
        
        Guid? SnapshotId { get; }
    }
}