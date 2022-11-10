using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal class ConfigureDocumentSynchronizationConfiguration : IConfigureDocumentSynchronizationConfiguration
    {
        private readonly IConfiguration _cache;

        public ConfigureDocumentSynchronizationConfiguration(IConfiguration cache)
        {
            _cache = cache;
        }

        public int DestinationWorkspaceArtifactId => _cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);

        public Guid ExportRunId
        {
            get
            {
                Guid snapshotId = _cache.GetFieldValue(x => x.SnapshotId) ?? Guid.Empty;
                return snapshotId != Guid.Empty
                    ? snapshotId
                    : throw new ArgumentException($"Run ID needs to be valid GUID, but null found.");
            }
        }

        public ImportOverwriteMode ImportOverwriteMode => _cache.GetFieldValue(x => x.ImportOverwriteMode);

        public FieldOverlayBehavior FieldOverlayBehavior => _cache.GetFieldValue(x => x.FieldOverlayBehavior);

        public bool ImageImport => _cache.GetFieldValue(x => x.ImageImport);

        public string FolderPathField => _cache.GetFieldValue(x => x.FolderPathSourceFieldName);

        public DestinationFolderStructureBehavior DestinationFolderStructureBehavior => _cache.GetFieldValue(x => x.DestinationFolderStructureBehavior);

        public int DataDestinationArtifactId => _cache.GetFieldValue(x => x.DataDestinationArtifactId);

        public ImportNativeFileCopyMode ImportNativeFileCopyMode => _cache.GetFieldValue(x => x.NativesBehavior);
    }
}
