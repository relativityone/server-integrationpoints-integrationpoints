using System;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.RelativitySync
{
    public class ExtendedJobForSyncApplication : IExtendedJob
    {
        public Job Job { get; set; }

        public long JobId { get; set; }

        public int WorkspaceId { get; set; }

        public int SubmittedById { get; set; }

        public int IntegrationPointId { get; set; }

        public IntegrationPoint IntegrationPointModel { get; set; }

        public Guid JobIdentifier { get; set; }

        public int JobHistoryId { get; set; }
    }
}