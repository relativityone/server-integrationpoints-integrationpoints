using System;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.RelativitySync
{
    public interface IExtendedJob
    {
        Job Job { get; }

        long JobId { get; }

        int WorkspaceId { get; }

        int SubmittedById { get; }

        int IntegrationPointId { get; }

        IntegrationPointDto IntegrationPointDto { get; }

        Guid JobIdentifier { get; }

        int JobHistoryId { get; }

        string ExecutingApplication { get; }
    }
}
