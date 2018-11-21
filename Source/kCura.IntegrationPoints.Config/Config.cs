﻿using System;
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
		private static readonly Lazy<Config> _instance = new Lazy<Config>(() => new Config());

		protected Config()
		{
		}

		public static Config Instance => _instance.Value;

		public string WebApiPath => GetValue(Constants.WEB_API_PATH, string.Empty);

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

		public bool MeasureDurationOfExternalCalls => GetValue(_RIP_METRICS_MEASURE_EXTERNAL_CALLS_DURATION, false);

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