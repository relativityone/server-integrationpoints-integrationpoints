using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.Core.Monitoring
{
    public static class HealthCheck
    {
        public static readonly string StuckJobMessage = "Integration Points Job Id {1} stuck in Workspace {0}.";
        public static readonly string InvalidJobMessage = "Jobs with invalid status found.";

        public static HealthCheckOperationResult CreateJobFailedMetric(JobHistory jobHistory, long workspaceId)
        {
            return new HealthCheckOperationResult($"Integration Points job failed! Job Id {jobHistory.JobID} in Workspace {workspaceId}!");
        }

        public static HealthCheckOperationResult CreateJobsWithInvalidStatusMetric(IDictionary<int, IList<JobHistory>> jobHistories)
        {
            Dictionary<string, object> customData = CreateJobHistoryCustomData(jobHistories);
            return new HealthCheckOperationResult(false, InvalidJobMessage, null, customData);
        }

        public static HealthCheckOperationResult CreateStuckJobsMetric(int workspaceId, IList<JobHistory> jobHistories)
        {
            var jobHistoriesDictionary = new Dictionary<int, IList<JobHistory>> { { workspaceId, jobHistories } };
            Dictionary<string, object> customData = CreateJobHistoryCustomData(jobHistoriesDictionary);
            string jobIdsMessage = BuildJobIdsMessage(jobHistories);
            string message = string.Format(StuckJobMessage, workspaceId, jobIdsMessage);
            return new HealthCheckOperationResult(false, message, null, customData);
        }

        private static string BuildJobIdsMessage(IList<JobHistory> jobHistories)
        {
            return string.Join(", ", jobHistories.Select(jobHistory => jobHistory.JobID));
        }

        private static Dictionary<string, object> CreateJobHistoryCustomData(IDictionary<int, IList<JobHistory>> jobHistories)
        {
            return jobHistories.ToDictionary(x => $"Workspace {x.Key}", y => (object)$"Job Ids: {string.Join(", ", y.Value.Select(z => z.JobID))}");
        }
    }
}