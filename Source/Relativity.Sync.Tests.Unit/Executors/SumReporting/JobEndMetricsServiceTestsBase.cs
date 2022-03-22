using Moq;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors.SumReporting
{
    public abstract class JobEndMetricsServiceTestsBase
    {
        internal Mock<IJobEndMetricsConfiguration> JobEndMetricsConfigurationFake;
        internal Mock<IBatchRepository> BatchRepositoryFake;
        internal Mock<IFieldManager> FieldManagerFake;
        internal Mock<ISyncMetrics> SyncMetricsMock;
        internal Mock<IJobStatisticsContainer> JobStatisticsContainerFake;

        public virtual void SetUp()
        {
            BatchRepositoryFake = new Mock<IBatchRepository>();
            JobEndMetricsConfigurationFake = new Mock<IJobEndMetricsConfiguration>(MockBehavior.Loose);
            FieldManagerFake = new Mock<IFieldManager>();
            SyncMetricsMock = new Mock<ISyncMetrics>();
            JobStatisticsContainerFake = new Mock<IJobStatisticsContainer>();
        }
    }
}
