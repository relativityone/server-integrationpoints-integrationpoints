﻿using System;
using System.Collections.Generic;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Configuration
{
    internal interface IConfigureDocumentSynchronizationConfiguration : IConfiguration
    {
        int DestinationWorkspaceArtifactId { get; }

        Guid ExportRunId { get; }

        ImportOverwriteMode ImportOverwriteMode { get; }

        FieldOverlayBehavior FieldOverlayBehavior { get; }

        bool ImageImport { get; }

        List<FieldInfoDto> FieldMappings { get; }

        string NativeFilePathField { get; }

        string NativeFileNameField { get; }

        string FolderPathField { get; }

        DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; }

        int DataDestinationArtifactId { get; }
    }
}
