using kCura.Apps.Common.Utils.Serializers;
using kCura.EDDS.WebAPI.FileManagerBase;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Telemetry;
using kCura.IntegrationPoints.Core.Telemetry.Metrics;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.Telemetry.Services.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static kCura.IntegrationPoints.Core.Contracts.Configuration.SourceConfiguration;

namespace kCura.IntegrationPoints.Core.Telemetry.Metrics
{
	public class ScheduleMetric : IMetric
	{
		private readonly string _bucket;

		private readonly int _integrationPointId;
		private readonly long _jobId;
		private readonly IScheduleRule _scheduleRule;
		private readonly ExportType _type;

		private ScheduleMetric(string bucket, int integrationPointId, long jobId, ExportType type, IScheduleRule scheduleRule)
		{
			_bucket = bucket;

			_integrationPointId = integrationPointId;
			_jobId = jobId;
			_type = type;
			_scheduleRule = scheduleRule;
		}

		public static ScheduleMetric CreateScheduleJobStarted(int integrationPointId, long jobId, ExportType type, IScheduleRule scheduleRule)
			=> IsScheduledDaily(scheduleRule)
				? new ScheduleMetric(MetricsBucket.SyncSchedule.SCHEDULE_SYNC_JOB_STARTED_DAILY, integrationPointId, jobId, type, scheduleRule)
				: new ScheduleMetric(MetricsBucket.SyncSchedule.SCHEDULE_SYNC_JOB_STARTED_NIGHTLY, integrationPointId, jobId, type, scheduleRule);

		public static ScheduleMetric CreateScheduleJobCompleted(int integrationPointId, long jobId, ExportType type, IScheduleRule scheduleRule)
			=> new ScheduleMetric(MetricsBucket.SyncSchedule.SCHEDULE_SYNC_JOB_COMPLETED, integrationPointId, jobId, type, scheduleRule);

		public static ScheduleMetric CreateScheduleJobFailed(int integrationPointId, long jobId, ExportType type, IScheduleRule scheduleRule)
			=> new ScheduleMetric(MetricsBucket.SyncSchedule.SCHEDULE_SYNC_JOB_FAILED, integrationPointId, jobId, type, scheduleRule);

		public async Task SendAsync(IMetricsManager metrics)
		{
			await metrics.LogCountAsync(_bucket, Guid.Empty, GetWorkflowId(), 1);
		}

		public bool CanSend() => _scheduleRule != null;

		private string GetWorkflowId() => $"Sync_{_type}_{_integrationPointId}_{_jobId}";

		private static bool IsScheduledDaily(IScheduleRule scheduleRule)
		{
			return true;
		}
	}
}
