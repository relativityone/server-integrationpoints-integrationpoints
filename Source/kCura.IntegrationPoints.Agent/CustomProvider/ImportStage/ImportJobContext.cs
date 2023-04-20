using System;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    internal class ImportJobContext
    {
        public ImportJobContext(Guid importJobId, long ripJobId, int workspaceId, int jobHistoryId)
        {
            ImportJobId = importJobId;
            RipJobId = ripJobId;
            WorkspaceId = workspaceId;
            JobHistoryId = jobHistoryId;
        }

        public Guid ImportJobId { get; }

        public long RipJobId { get; }

        public int WorkspaceId { get; }

        public int JobHistoryId { get; }
    }
}
