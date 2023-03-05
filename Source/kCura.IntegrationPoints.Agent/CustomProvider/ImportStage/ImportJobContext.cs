using System;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    internal class ImportJobContext
    {
        public ImportJobContext(Guid importJobId, long ripJobId, int destinationWorkspaceId)
        {
            ImportJobId = importJobId;
            RipJobId = ripJobId;
            DestinationWorkspaceId = destinationWorkspaceId;
        }

        public Guid ImportJobId { get; }

        public long RipJobId { get; }

        public int DestinationWorkspaceId { get; }
    }
}
