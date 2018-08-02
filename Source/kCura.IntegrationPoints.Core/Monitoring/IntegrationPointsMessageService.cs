using System;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Monitoring.JobLifetimeMessages;
using kCura.IntegrationPoints.Core.Monitoring.NumberOfRecords.Messages;
using kCura.IntegrationPoints.Core.Monitoring.NumberOfRecordsMessages;
using kCura.IntegrationPoints.Core.Monitoring.Sinks.Aggregated;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Relativity.DataTransfer.MessageService.Tools;

namespace kCura.IntegrationPoints.Core.Monitoring
{
	public class IntegrationPointsMessageService : MessageService
	{
		public IntegrationPointsMessageService(IMetricsManagerFactory metricsManagerFactory, IConfig config, IHelper helper)
		{
			var aggregatedJobSink = new AggregatedJobSink(helper, metricsManagerFactory);

			this.AddSink(new ToggledMessageSink<JobStartedMessage>(aggregatedJobSink, () => config.SendSumMetrics));
			this.AddSink(new ToggledMessageSink<JobCompletedMessage>(aggregatedJobSink, () => config.SendSumMetrics));
			this.AddSink(new ToggledMessageSink<JobFailedMessage>(aggregatedJobSink, () => config.SendSumMetrics));
			this.AddSink(new ToggledMessageSink<JobValidationFailedMessage>(aggregatedJobSink, () => config.SendSumMetrics));
			this.AddSink(new ToggledMessageSink<JobTotalRecordsCountMessage>(aggregatedJobSink, () => config.SendSumMetrics));
			this.AddSink(new ToggledMessageSink<JobCompletedRecordsCountMessage>(aggregatedJobSink, () => config.SendSumMetrics));
			this.AddSink(new ToggledMessageSink<JobThroughputMessage>(aggregatedJobSink, () => config.SendSumMetrics));
			this.AddSink(new ToggledMessageSink<ExportJobThroughputBytesMessage>(aggregatedJobSink, () => config.SendSumMetrics));
			this.AddSink(new ToggledMessageSink<ExportJobStatisticsMessage>(aggregatedJobSink, () => config.SendSummaryMetrics));
			this.AddSink(new ToggledMessageSink<JobProgressMessage>(new ThrottledMessageSink<JobProgressMessage>(aggregatedJobSink, () => config.MetricsThrottling), () => config.SendLiveApmMetrics));
		}
	}
}