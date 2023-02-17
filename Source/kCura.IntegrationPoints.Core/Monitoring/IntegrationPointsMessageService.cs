using kCura.IntegrationPoints.Common.Metrics;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation.Model;
using kCura.IntegrationPoints.Common.Monitoring.Messages;
using kCura.IntegrationPoints.Common.Monitoring.Messages.JobLifetime;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Monitoring.MessageSink.Aggregated;
using kCura.IntegrationPoints.Core.Monitoring.MessageSink.ExternalCalls;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Relativity.DataTransfer.MessageService.Tools;

namespace kCura.IntegrationPoints.Core.Monitoring
{
    public class IntegrationPointsMessageService : MessageService
    {
        public IntegrationPointsMessageService(IMetricsManagerFactory metricsManagerFactory, IConfig config, IAPILog logger, IDateTimeHelper dateTimeHelper, IRipMetrics ripMetrics)
        {
            ConfigureAggregatedJobSink(metricsManagerFactory, config, logger, dateTimeHelper, ripMetrics);
            ConfigureExternalCallsSink(metricsManagerFactory, config, logger);
        }

        private void ConfigureAggregatedJobSink(IMetricsManagerFactory metricsManagerFactory, IConfig config, IAPILog logger, IDateTimeHelper dateTimeHelper, IRipMetrics ripMetrics)
        {
            var aggregatedJobSink = new AggregatedJobSink(logger, metricsManagerFactory, dateTimeHelper, ripMetrics);

            this.AddSink(new ToggledMessageSink<JobStartedMessage>(aggregatedJobSink, () => config.SendSumMetrics));
            this.AddSink(new ToggledMessageSink<JobCompletedMessage>(aggregatedJobSink, () => config.SendSumMetrics));
            this.AddSink(new ToggledMessageSink<JobFailedMessage>(aggregatedJobSink, () => config.SendSumMetrics));
            this.AddSink(new ToggledMessageSink<JobValidationFailedMessage>(aggregatedJobSink, () => config.SendSumMetrics));
            this.AddSink(new ToggledMessageSink<JobSuspendedMessage>(aggregatedJobSink, () => config.SendSumMetrics));
            this.AddSink(new ToggledMessageSink<JobTotalRecordsCountMessage>(aggregatedJobSink, () => config.SendSumMetrics));
            this.AddSink(new ToggledMessageSink<JobCompletedRecordsCountMessage>(aggregatedJobSink, () => config.SendSumMetrics));
            this.AddSink(new ToggledMessageSink<JobThroughputMessage>(aggregatedJobSink, () => config.SendSumMetrics));
            this.AddSink(new ToggledMessageSink<JobThroughputBytesMessage>(aggregatedJobSink, () => config.SendSumMetrics));
            this.AddSink(new ToggledMessageSink<JobStatisticsMessage>(aggregatedJobSink, () => config.SendSummaryMetrics));
            this.AddSink(new ToggledMessageSink<JobProgressMessage>(new ThrottledMessageSink<JobProgressMessage>(aggregatedJobSink, () => config.MetricsThrottling), () => config.SendLiveApmMetrics));
        }

        private void ConfigureExternalCallsSink(IMetricsManagerFactory metricsManagerFactory, IConfig config, IAPILog logger)
        {
            var externalCallsSink = new ExternalCallsSink(metricsManagerFactory, logger);

            this.AddSink(new ToggledMessageSink<ExternalCallCompletedMessage>(externalCallsSink, () => config.SendLiveApmMetrics));
            this.AddSink(new ToggledMessageSink<JobStartedMessage>(externalCallsSink, () => config.SendLiveApmMetrics));
            this.AddSink(new ToggledMessageSink<JobCompletedMessage>(externalCallsSink, () => config.SendLiveApmMetrics));
            this.AddSink(new ToggledMessageSink<JobFailedMessage>(externalCallsSink, () => config.SendLiveApmMetrics));
        }
    }
}
