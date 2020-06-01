using kCura.IntegrationPoints.Core.Telemetry;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;
using Relativity.Telemetry.Services.Metrics;
using System;
using System.Threading.Tasks;
using static kCura.IntegrationPoints.Core.Contracts.Configuration.SourceConfiguration;

namespace kCura.IntegrationPoints.RelativitySync.Metrics
{
	public class ScheduleMetric : IMetric
	{
		private readonly IServicesMgr _servicesMgr;

		private readonly string _bucket;

		private readonly int _integrationPointId;
		private readonly long _jobId;
		private readonly ExportType _type;

		private ScheduleMetric(IServicesMgr servicesMgr, string bucket, int integrationPointId, long jobId, ExportType type)
		{
			_servicesMgr = servicesMgr;

			_bucket = bucket;

			_integrationPointId = integrationPointId;
			_jobId = jobId;
			_type = type;
		}

		public static ScheduleMetric CreateScheduleJobStarted(IServicesMgr servicesMgr, int integrationPointId, long jobId, ExportType type, IScheduleRule scheduleRule)
			=> IsScheduledDaily(scheduleRule)
				? new ScheduleMetric(servicesMgr, MetricsBucket.SyncSchedule.SCHEDULE_SYNC_JOB_STARTED_DAILY, integrationPointId, jobId, type)
				: new ScheduleMetric(servicesMgr, MetricsBucket.SyncSchedule.SCHEDULE_SYNC_JOB_STARTED_NIGHTLY, integrationPointId, jobId, type);

		public static ScheduleMetric CreateScheduleJobCompleted(IServicesMgr servicesMgr, int integrationPointId, long jobId, ExportType type)
			=> new ScheduleMetric(servicesMgr, MetricsBucket.SyncSchedule.SCHEDULE_SYNC_JOB_COMPLETED, integrationPointId, jobId, type);

		public static ScheduleMetric CreateScheduleJobFailed(IServicesMgr servicesMgr, int integrationPointId, long jobId, ExportType type)
			=> new ScheduleMetric(servicesMgr, MetricsBucket.SyncSchedule.SCHEDULE_SYNC_JOB_FAILED, integrationPointId, jobId, type);

		public async Task SendAsync()
		{
			using (var metrics = _servicesMgr.CreateProxy<IMetricsManager>(ExecutionIdentity.System))
			{
				await metrics.LogCountAsync(_bucket, Guid.Empty, GetWorkflowId(), 1);
			}
		}

		private string GetWorkflowId() => $"Sync_{_type}_{_integrationPointId}_{_jobId}";

		private static bool IsScheduledDaily(IScheduleRule scheduleRule)
		{
			DateTime scheduleTime = scheduleRule.GetNextUTCRunDateTime().Value;

			TimeSpan startOfDay = new TimeSpan(6, 0, 0);
			TimeSpan endOfDay = new TimeSpan(18, 0, 0);

			return startOfDay <= scheduleTime.TimeOfDay && scheduleTime.TimeOfDay <= endOfDay;
		}
	}
}
