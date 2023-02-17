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
        private const int _SIX_AM = 6;
        private const int _SIX_PM = 18;
        private readonly IServicesMgr _servicesMgr;

        public string Bucket { get; }

        public int IntegrationPointId { get; }

        public long JobId { get; }

        public ExportType Type { get; }

        private ScheduleMetric(IServicesMgr servicesMgr, string bucket, int integrationPointId, long jobId, ExportType type)
        {
            _servicesMgr = servicesMgr;

            Bucket = bucket;

            IntegrationPointId = integrationPointId;
            JobId = jobId;
            Type = type;
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
                await metrics.LogCountAsync(Bucket, Guid.Empty, GetWorkflowId(), 1).ConfigureAwait(false);
            }
        }

        private string GetWorkflowId() => $"Sync_{Type}_{IntegrationPointId}_{JobId}";
        private static bool IsScheduledDaily(IScheduleRule scheduleRule)
        {
            DateTime scheduleTime = scheduleRule.GetNextUTCRunDateTime().Value;

            TimeSpan startOfDay = new TimeSpan(_SIX_AM, 0, 0);
            TimeSpan endOfDay = new TimeSpan(_SIX_PM, 0, 0);

            return startOfDay <= scheduleTime.TimeOfDay && scheduleTime.TimeOfDay <= endOfDay;
        }
    }
}
