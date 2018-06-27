using System;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Monitoring.JobLifetimeMessages;
using kCura.IntegrationPoints.Core.Monitoring.NumberOfRecords.Messages;
using kCura.IntegrationPoints.Core.Monitoring.NumberOfRecordsMessages;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using Relativity.DataTransfer.MessageService;
using Relativity.DataTransfer.MessageService.MetricsManager.APM;
using Relativity.DataTransfer.MessageService.Tools;

namespace kCura.IntegrationPoints.Core.Monitoring
{
	public class IntegrationPointsMessageService : MessageService
	{
		public IntegrationPointsMessageService(IMetricsManagerFactory metricsManagerFactory, IConfig config)
		{
			var sumMetricSink = new SumMetricSink(metricsManagerFactory); 
			
			this.AddSink(new ToggledMessageSink<JobStartedMessage>(sumMetricSink,  () => config.SendSumMetrics));
			this.AddSink(new ToggledMessageSink<JobCompletedMessage>(sumMetricSink, () => config.SendSumMetrics));
			this.AddSink(new ToggledMessageSink<JobFailedMessage>(sumMetricSink, () => config.SendSumMetrics));
			this.AddSink(new ToggledMessageSink<JobValidationFailedMessage>(sumMetricSink, () => config.SendSumMetrics));
			this.AddSink(new ToggledMessageSink<JobTotalRecordsCountMessage>(sumMetricSink, () => config.SendSumMetrics));
			this.AddSink(new ToggledMessageSink<JobCompletedRecordsCountMessage>(sumMetricSink, () => config.SendSumMetrics));
			this.AddSink(new ToggledMessageSink<JobThroughputMessage>(sumMetricSink, () => config.SendSumMetrics));

			var liveApmMetricSink = new LiveApmMetricSink(metricsManagerFactory);

			this.AddSink(new ToggledMessageSink<JobApmThroughputMessage>(new ThrottledMessageSink<JobApmThroughputMessage>(liveApmMetricSink, () => config.MetricsThrottling), () => config.SendLiveApmMetrics));

			var endApmMetricSink = new EndApmMetricSink(metricsManagerFactory);

			this.AddSink(new ToggledMessageSink<ImportJobStatisticsMessage>(endApmMetricSink, () => config.SendSummaryMetrics));
			this.AddSink(new ToggledMessageSink<ExportJobStatisticsMessage>(endApmMetricSink, () => config.SendSummaryMetrics));
		}
	}
}