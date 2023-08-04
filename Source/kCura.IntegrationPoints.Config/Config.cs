using System;

namespace kCura.IntegrationPoints.Config
{
    public class Config : ConfigBase, IConfig
    {
        private const int _BATCH_SIZE_DEFAULT = 1000;
        private const string _BATCH_SIZE = "BatchSize";
        private const string _DISABLE_NATIVE_LOCATION_VALIDATION = "DisableNativeLocationValidation";
        private const int _MAX_FAILED_SCHEDULED_JOBS_COUNT_DEFAULT = 10;
        private const string _MAX_FAILED_SCHEDULED_JOBS_COUNT = "MaxFailedScheduledJobsCount";
        private const string _DISABLE_NATIVE_VALIDATION = "DisableNativeValidation";
        private const string _RIP_METRICS_CONFIGURATION = "MetricsConfiguration";
        private const string _RIP_METRICS_THROTTLING = "MetricsThrottlingSeconds";
        private const string _RIP_METRICS_MEASURE_EXTERNAL_CALLS_DURATION = "MeasureDurationOfExternalCalls";
        private const string _MASS_UPDATE_BATCH_SIZE = "MassUpdateBatchSize";
        private const string _LONG_RUNNING_JOBS_TIME_THRESHOLD = "LongRunningJobsTimeThreshold";
        private const int _MASS_UPDATE_BATCH_SIZE_DEFAULT = 10000;
        private const string _TRANSIENT_STATE_RIP_JOB_TIMEOUT = "TransientStateJobTimeout";
        private const int _TRANSIENT_STATE_RIP_JOB_TIMEOUT_DEFAULT_IN_MINUTES = 30;
        private const string _AGENT_MAXIMUM_LIFETIME = "AgentMaximumLifetime";
        private const int _AGENT_MAXIMUM_LIFETIME_DEFAULT_IN_MINUTES = 3 * 60;
        private static readonly Lazy<Config> _instance = new Lazy<Config>(() => new Config());

        protected Config()
        {
        }

        public static Config Instance => _instance.Value;

        public bool DisableNativeLocationValidation => GetValue(_DISABLE_NATIVE_LOCATION_VALIDATION, false);

        public bool DisableNativeValidation => GetValue(_DISABLE_NATIVE_VALIDATION, false);

        public TimeSpan MetricsThrottling => TimeSpan.FromSeconds(GetValue(_RIP_METRICS_THROTTLING, 30));

        public bool SendLiveApmMetrics => GetMetricsToggle(Metrics.LiveApmMetrics);

        public bool SendSummaryMetrics => GetMetricsToggle(Metrics.SummaryApmMetrics);

        public bool SendSumMetrics => GetMetricsToggle(Metrics.SumMetrics);

        public int BatchSize
        {
            get
            {
                int value = GetValue(_BATCH_SIZE, _BATCH_SIZE_DEFAULT);
                return value >= 0 ? value : _BATCH_SIZE_DEFAULT;
            }
        }

        public int MaxFailedScheduledJobsCount
        {
            get
            {
                int value = GetValue(_MAX_FAILED_SCHEDULED_JOBS_COUNT, _MAX_FAILED_SCHEDULED_JOBS_COUNT_DEFAULT);
                return value > 0 ? value : _MAX_FAILED_SCHEDULED_JOBS_COUNT_DEFAULT;
            }
        }

        public bool MeasureDurationOfExternalCalls => GetValue(_RIP_METRICS_MEASURE_EXTERNAL_CALLS_DURATION, false);

        public int MassUpdateBatchSize => GetValue(
            _MASS_UPDATE_BATCH_SIZE,
            _MASS_UPDATE_BATCH_SIZE_DEFAULT
            );

        public TimeSpan RunningJobTimeThreshold => TimeSpan.FromSeconds(GetValue(_LONG_RUNNING_JOBS_TIME_THRESHOLD, 28800));

        public TimeSpan TransientStateJobTimeout
        {
            get
            {
                int value = GetValue(_TRANSIENT_STATE_RIP_JOB_TIMEOUT, _TRANSIENT_STATE_RIP_JOB_TIMEOUT_DEFAULT_IN_MINUTES);
                return TimeSpan.FromMinutes(value);
            }
        }

        public TimeSpan AgentMaximumLifetime
        {
            get
            {
                int value = GetValue(_AGENT_MAXIMUM_LIFETIME, _AGENT_MAXIMUM_LIFETIME_DEFAULT_IN_MINUTES);
                return TimeSpan.FromMinutes(value);
            }
        }

        private bool GetMetricsToggle(Metrics metricName)
        {
            return (GetValue(_RIP_METRICS_CONFIGURATION, int.MaxValue) & (int)metricName) > 0;
        }

        private enum Metrics
        {
            LiveApmMetrics = 1,
            SummaryApmMetrics = 2,
            SumMetrics = 4
        }
    }
}
