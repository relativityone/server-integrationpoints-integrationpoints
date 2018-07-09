using kCura.IntegrationPoints.Core.Monitoring.NumberOfRecords.Messages;
using Relativity.DataTransfer.MessageService;
using Relativity.DataTransfer.MessageService.Tools;

namespace kCura.IntegrationPoints.Core.Monitoring
{
	public class LiveApmMetricSink : IMessageSink<JobApmThroughputMessage>
	{
		private readonly IMetricsManagerFactory _metricsManagerFactory;

		public LiveApmMetricSink(IMetricsManagerFactory metricsManagerFactory)
		{
			_metricsManagerFactory = metricsManagerFactory;
		}

		public void OnMessage(JobApmThroughputMessage message)
		{
			_metricsManagerFactory.CreateAPMManager().LogDouble("IntegrationPoints.Performance.Progress", 1, message);
		}
	}
}