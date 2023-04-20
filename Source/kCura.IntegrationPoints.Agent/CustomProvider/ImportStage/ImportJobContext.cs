using System;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    internal class ImportJobContext
    {
        public ImportJobContext(int workspaceId, long ripJobId, Guid jobHistoryGuid, int jobHistoryId)
        {
            WorkspaceId = workspaceId;
            RipJobId = ripJobId;
            JobHistoryGuid = jobHistoryGuid;
            JobHistoryId = jobHistoryId;
        }

        public int WorkspaceId { get; }

        public long RipJobId { get; }

        public Guid JobHistoryGuid { get; }

        public int JobHistoryId { get; }
    }
}
