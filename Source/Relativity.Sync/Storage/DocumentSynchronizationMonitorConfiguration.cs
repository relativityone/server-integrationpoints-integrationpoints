using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal class DocumentSynchronizationMonitorConfiguration : IDocumentSynchronizationMonitorConfiguration
    {
        private readonly IConfiguration _cache;

        public DocumentSynchronizationMonitorConfiguration(IConfiguration cache)
        {
            _cache = cache;
        }

        public Guid ExportRunId
        {
            get
            {
                Guid? snapshotId = _cache.GetFieldValue(x => x.SnapshotId);
                if (snapshotId == Guid.Empty)
                {
                    snapshotId = null;
                }

                return snapshotId ?? throw new ArgumentException($"Run ID needs to be valid GUID, but null found.");
            }
        }

        public int DestinationWorkspaceArtifactId => _cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);
    }
}
