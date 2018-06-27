using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using Relativity.DataTransfer.MessageService;
using Relativity.DataTransfer.MessageService.MetricsManager.APM;
using Relativity.DataTransfer.MessageService.Tools;

namespace kCura.IntegrationPoints.Core.Monitoring
{
	public class EndApmMetricSink : IMessageSink<ImportJobStatisticsMessage>, IMessageSink<ExportJobStatisticsMessage>
	{
		private readonly IMetricsManagerFactory _metricsManagerFactory;

		public EndApmMetricSink(IMetricsManagerFactory metricsManagerFactory)
		{
			_metricsManagerFactory = metricsManagerFactory;
		}

		public void OnMessage(ImportJobStatisticsMessage message)
		{
			LogJobStatistics(message.FileBytes + message.MetaBytes, message);
		}

		public void OnMessage(ExportJobStatisticsMessage message)
		{
			LogJobStatistics(message.FileBytes + message.MetaBytes, message);
		}

		private void LogJobStatistics(double totalSize, IMetricMetadata metricMetadata)
		{
			_metricsManagerFactory.CreateAPMManager().LogDouble("IntegrationPoints.Performance.JobStatistics", totalSize, metricMetadata);
		}
	}
}