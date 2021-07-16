using kCura.IntegrationPoints.Data;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers
{
    internal static class PerformanceTestsConstants
    {
        public const int MAX_DURATION_MS = 600000; // 10 minutes
        public const int RUN_COUNT = 5;

        public const string PERFORMANCE_TEST_WORKSPACE_NAME_FORMAT = "RIP_PERFORMANCE_{0}";

        public static string JOB_STATUS_PENDING = JobStatusChoices.JobHistoryPending.Name;
        public static string JOB_STATUS_PROCESSING = JobStatusChoices.JobHistoryProcessing.Name;
        public static string JOB_STATUS_VALIDATING = JobStatusChoices.JobHistoryValidating.Name;
    }
}