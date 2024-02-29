using System.Collections.Generic;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Configuration
{
    internal interface IFieldConfiguration
    {
        DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; }

        int SourceWorkspaceArtifactId { get; }

        int DestinationWorkspaceArtifactId { get; }

        int RdoArtifactTypeId { get; }

        int DestinationRdoArtifactTypeId { get; }

        string GetFolderPathSourceFieldName();

        IList<FieldMap> GetFieldMappings();

        ImportNativeFileCopyMode ImportNativeFileCopyMode { get; }
    }
}
