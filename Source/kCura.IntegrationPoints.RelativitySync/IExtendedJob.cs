using System;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.RelativitySync
{
    public interface IExtendedJob
    {
        Job Job { get; }
        long JobId { get; }
        int WorkspaceId { get; }
        int SubmittedById { get; }
        int IntegrationPointId { get; }
        IntegrationPoint IntegrationPointModel { get; }
        Guid JobIdentifier { get; }
        int JobHistoryId { get; }
    }
}