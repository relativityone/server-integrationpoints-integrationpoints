using System;

namespace Relativity.Sync.Configuration
{
    internal interface IConfigureDocumentSynchronizationConfiguration : IConfiguration
    {
        int DestinationWorkspaceArtifactId { get; }

        Guid ExportRunId { get; }

        ImportOverwriteMode ImportOverwriteMode { get; }

        FieldOverlayBehavior FieldOverlayBehavior { get; }

        ImportNativeFileCopyMode ImportNativeFileCopyMode { get; }

        bool ImageImport { get; }

        string FolderPathSourceFieldName { get; }

        DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; }

        bool MoveExistingDocuments { get; }

        int DataDestinationArtifactId { get; }
    }
}
