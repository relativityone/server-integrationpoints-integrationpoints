using kCura.IntegrationPoints.Data;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers
{
    internal static class PerformanceTestsConstants
    {
        public const int MAX_AVERAGE_DURATION_S = 120; // 2 minutes
        public const int RUN_COUNT = 5;
        public const string PERFORMANCE_TEST_WORKSPACE_NAME_FORMAT = "RIP_PERFORMANCE_{0}";
        public const string PERFORMANCE_TEST_INTEGRATION_POINT_NAME_FORMAT = "RIP_PERFORMANCE_{0}";

        public static string JOB_STATUS_PENDING = JobStatusChoices.JobHistoryPending.Name;

        public static string JOB_STATUS_PROCESSING = JobStatusChoices.JobHistoryProcessing.Name;

        public static string JOB_STATUS_VALIDATING = JobStatusChoices.JobHistoryValidating.Name;
    }
}
