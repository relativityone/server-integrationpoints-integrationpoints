using Relativity.Services.InternalMetricsCollection;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Telemetry
{
	public static class MetricsBucket
	{
		public static class SyncSchedule
		{
			public const string SCHEDULE_CATEGORY = "Integration Points Sync Schedule";

			public const string SCHEDULE_SYNC_JOB_STARTED_DAILY = "Relativity.Sync.Schedule.JobStarted.Daily";
			public const string SCHEDULE_SYNC_JOB_STARTED_NIGHTLY = "Relativity.Sync.Schedule.JobStarted.Nightly";
			public const string SCHEDULE_SYNC_JOB_COMPLETED = "Relativity.Sync.Schedule.JobCompleted";
			public const string SCHEDULE_SYNC_JOB_FAILED = "Relativity.Sync.Schedule.Job.Failed";

			public static readonly List<MetricIdentifier> SCHEDULE_METRICS = new List<MetricIdentifier>
			{
				new MetricIdentifier
				{
					Name = SCHEDULE_SYNC_JOB_STARTED_DAILY,
					Description = "Number of Sync jobs scheduled during working day (6:00 AM - 6:00 PM)"
				},
				new MetricIdentifier
				{
					Name = SCHEDULE_SYNC_JOB_STARTED_NIGHTLY,
					Description = "Number of Sync jobs scheduled during working day (6:00 PM - 6:00 AM)"
				},
				new MetricIdentifier
				{
					Name = SCHEDULE_SYNC_JOB_COMPLETED,
					Description = "Number of Sync scheduled jobs completed"
				},
				new MetricIdentifier
				{
					Name = SCHEDULE_SYNC_JOB_FAILED,
					Description = "	Number of Sync scheduled jobs failed"
				}
			};
		}
	}
}
