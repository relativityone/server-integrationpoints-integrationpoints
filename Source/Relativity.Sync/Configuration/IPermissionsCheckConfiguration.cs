using System;

namespace Relativity.Sync.Configuration
{
    internal interface IPermissionsCheckConfiguration : IConfiguration
    {
        int SourceWorkspaceArtifactId { get; }

        int DestinationWorkspaceArtifactId { get; }

        int DestinationFolderArtifactId { get;  }

        int SourceProviderArtifactId { get; }

        bool CreateSavedSearchForTags { get; }

        Guid JobHistoryObjectTypeGuid { get; }
        
        int RdoArtifactTypeId { get; }

        int DestinationRdoArtifactTypeId { get; }
        
        ImportOverwriteMode ImportOverwriteMode { get; }
    }
}