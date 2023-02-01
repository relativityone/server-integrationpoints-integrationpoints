using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal class LoadFileConfiguration : ILoadFileConfiguration
    {
        private readonly IConfiguration _cache;

        public LoadFileConfiguration(IConfiguration cache)
        {
            _cache = cache;
        }

        public int DestinationWorkspaceArtifactId => _cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);

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
    }
}
