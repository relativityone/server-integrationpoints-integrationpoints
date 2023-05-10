using System;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
    public interface IJobStatisticsService
    {
        void Subscribe(IBatchReporter reporter, Job job);

        void SetIntegrationPointConfiguration(DestinationConfiguration destinationConfiguration, SourceConfiguration sourceConfiguration);

        void Update(Guid identifier, int transferredItem, int erroredCount);
    }
}
