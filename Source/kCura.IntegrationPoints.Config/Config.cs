using System;
using kCura.IntegrationPoints.Domain;

namespace kCura.IntegrationPoints.Config
{
	public class Config : ConfigBase, IConfig
	{
		private const int _BATCH_SIZE_DEFAULT = 1000;
		private const string _BATCH_SIZE = "BatchSize";
		private const string _DISABLE_NATIVE_LOCATION_VALIDATION = "DisableNativeLocationValidation";
		private const string _DISABLE_NATIVE_VALIDATION = "DisableNativeValidation";
		private const string _RIP_METRICS_CONFIGURATION = "MetricsConfiguration";
		private const string _RIP_METRICS_THROTTLING = "MetricsThrottlingSeconds";
		private const string _RIP_METRICS_MEASURE_EXTERNAL_CALLS_DURATION = "MeasureDurationOfExternalCalls";
		private const string _MASS_UPDATE_BATCH_SIZE = "MassUpdateBatchSize";
		private const int _MASS_UPDATE_BATCH_SIZE_DEFAULT = 10000;

		private const string _TRANSIENT_STATE_RIP_JOB_TIMEOUT = "TransientStateJobTimeout";
		private const int _TRANSIENT_STATE_RIP_JOB_TIMEOUT_DEFAULT_IN_MINUTES = 8 * 60;


		private const string _RELATIVITY_WEBAPI_TIMEOUT_SETTING_NAME = "RelativityWebApiTimeout";

		private static readonly Lazy<Config> _instance = new Lazy<Config>(() => new Config());

		protected Config()
		{
		}

		public static Config Instance => _instance.Value;

		public string WebApiPath => GetValue(Constants.WEB_API_PATH, string.Empty);

		public bool DisableNativeLocationValidation => GetValue(_DISABLE_NATIVE_LOCATION_VALIDATION, false);

		public bool DisableNativeValidation => GetValue(_DISABLE_NATIVE_VALIDATION, false);

		public TimeSpan? RelativityWebApiTimeout
		{
			get
			{
				if (_instanceSettings.Value.Contains(_RELATIVITY_WEBAPI_TIMEOUT_SETTING_NAME))
				{
					return TimeSpan.FromSeconds(GetValue<int>(_RELATIVITY_WEBAPI_TIMEOUT_SETTING_NAME));
				}
				else
				{
					return null;
				}
			}
		}

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

		public bool MeasureDurationOfExternalCalls => GetValue(_RIP_METRICS_MEASURE_EXTERNAL_CALLS_DURATION, false);

		public int MassUpdateBatchSize => GetValue(
			_MASS_UPDATE_BATCH_SIZE,
			_MASS_UPDATE_BATCH_SIZE_DEFAULT
			);

		public TimeSpan TransientStateJobTimeout
		{
			get
			{
				int value = GetValue(_TRANSIENT_STATE_RIP_JOB_TIMEOUT, _TRANSIENT_STATE_RIP_JOB_TIMEOUT_DEFAULT_IN_MINUTES);
				return TimeSpan.FromMinutes(value);
			}
		}

		private bool GetMetricsToggle(Metrics metricName)
		{
			return (GetValue(_RIP_METRICS_CONFIGURATION, Int32.MaxValue) & (int)metricName) > 0;
		}

		private enum Metrics
		{
			LiveApmMetrics = 1,
			SummaryApmMetrics = 2,
			SumMetrics = 4
		}
	}
}