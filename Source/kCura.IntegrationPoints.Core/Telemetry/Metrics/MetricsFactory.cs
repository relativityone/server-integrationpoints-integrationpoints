using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Telemetry.Metrics;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using static kCura.IntegrationPoints.Core.Contracts.Configuration.SourceConfiguration;

namespace kCura.IntegrationPoints.Core.Telemetry
{
	public class MetricsFactory : IMetricsFactory
	{
		private readonly ISerializer _serializer;
		private readonly IIntegrationPointService _integrationPointService;
		private readonly IScheduleRuleFactory _scheduleRuleFactory;

		public MetricsFactory(ISerializer serializer, IScheduleRuleFactory scheduleRuleFactory, IIntegrationPointService integrationPointService)
		{
			_serializer = serializer;
			_scheduleRuleFactory = scheduleRuleFactory;
			_integrationPointService = integrationPointService;
		}

		public IMetric CreateScheduleJobStartedMetric(Job job)
		{
			IntegrationPoint integrationPoint = _integrationPointService.ReadIntegrationPoint(job.RelatedObjectArtifactID);
			ExportType type = _serializer.Deserialize<SourceConfiguration>(integrationPoint.SourceConfiguration).TypeOfExport;
			IScheduleRule scheduleRule = _scheduleRuleFactory.Deserialize(job);

			return ScheduleMetric.CreateScheduleJobStarted(integrationPoint.ArtifactId, job.JobId, type, scheduleRule);
		}

		public IMetric CreateScheduleJobCompletedMetric(Job job)
		{
			IntegrationPoint integrationPoint = _integrationPointService.ReadIntegrationPoint(job.RelatedObjectArtifactID);
			ExportType type = _serializer.Deserialize<SourceConfiguration>(integrationPoint.SourceConfiguration).TypeOfExport;
			IScheduleRule scheduleRule = _scheduleRuleFactory.Deserialize(job);

			return ScheduleMetric.CreateScheduleJobCompleted(integrationPoint.ArtifactId, job.JobId, type, scheduleRule);
		}

		public IMetric CreateScheduleJobFailedMetric(Job job)
		{
			IntegrationPoint integrationPoint = _integrationPointService.ReadIntegrationPoint(job.RelatedObjectArtifactID);
			ExportType type = _serializer.Deserialize<SourceConfiguration>(integrationPoint.SourceConfiguration).TypeOfExport;
			IScheduleRule scheduleRule = _scheduleRuleFactory.Deserialize(job);

			return ScheduleMetric.CreateScheduleJobFailed(integrationPoint.ArtifactId, job.JobId, type, scheduleRule);
		}
	}
}
