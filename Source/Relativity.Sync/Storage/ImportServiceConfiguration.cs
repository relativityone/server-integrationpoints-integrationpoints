using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal class ImportServiceConfiguration : IImportServiceConfiguration
    {
        private readonly IConfiguration _cache;

        public ImportServiceConfiguration(IConfiguration cache)
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
    }
}
