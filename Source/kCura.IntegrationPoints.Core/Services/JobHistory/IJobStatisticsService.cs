using System;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
    public interface IJobStatisticsService
    {
        void Subscribe(IBatchReporter reporter, Job job);
        void SetIntegrationPointConfiguration(ImportSettings importSettings, SourceConfiguration sourceConfiguration);
        void Update(Guid identifier, int transferredItem, int erroredCount);
    }
}