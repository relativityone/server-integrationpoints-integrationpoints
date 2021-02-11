using System.Collections.Generic;

namespace Relativity.Sync.Telemetry.Metrics
{
	internal class KeplerMetric : IMetric
	{
		public string Name { get; }

		public string WorkflowId { get; set; }

		public ExecutionStatus? ExecutionStatus { get; set; }

		public double? Duration { get; set; }

		public long? NumberOfHttpRetriesForSuccess { get; set; }

		public long? NumberOfHttpRetriesForFailed { get; set; }

		public long? AuthTokenExpirationCount { get; set; }

		public KeplerMetric(string invocationKepler)
		{
			Name = invocationKepler;
		}

		public Dictionary<string, object> GetCustomData() =>
			new Dictionary<string, object>
			{
				{nameof(Name), Name},
				{nameof(WorkflowId), WorkflowId},
				{nameof(ExecutionStatus), ExecutionStatus},
				{nameof(Duration), Duration},
				{nameof(NumberOfHttpRetriesForSuccess), NumberOfHttpRetriesForSuccess},
				{nameof(NumberOfHttpRetriesForFailed), NumberOfHttpRetriesForFailed},
				{nameof(AuthTokenExpirationCount), AuthTokenExpirationCount},
			};

		public IEnumerable<SumMetric> GetSumMetrics()
		{
			var sumMetrics = new List<SumMetric>();

			if (Duration != null)
			{
				sumMetrics.Add(new SumMetric
				{
					Bucket = BucketWithNamePrefix(TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_DURATION_SUFFIX),
					Type = MetricType.TimedOperation,
					Value = Duration,
					WorkflowId = WorkflowId
				});
			}
			if (NumberOfHttpRetriesForSuccess != null)
			{
				sumMetrics.Add(new SumMetric
				{
					Bucket = BucketWithNamePrefix(TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_SUCCESS_SUFFIX),
					Type = MetricType.PointInTimeLong,
					Value = NumberOfHttpRetriesForSuccess,
					WorkflowId = WorkflowId
				});
			}
			if (NumberOfHttpRetriesForFailed != null)
			{
				sumMetrics.Add(new SumMetric
				{
					Bucket = BucketWithNamePrefix(TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_FAILED_SUFFIX),
					Type = MetricType.PointInTimeLong,
					Value = NumberOfHttpRetriesForFailed,
					WorkflowId = WorkflowId
				});
			}
			if (AuthTokenExpirationCount != null)
			{
				sumMetrics.Add(new SumMetric
				{
					Bucket = BucketWithNamePrefix(TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_AUTH_REFRESH_SUFFIX),
					Type = MetricType.PointInTimeLong,
					Value = AuthTokenExpirationCount,
					WorkflowId = WorkflowId
				});
			}

			return sumMetrics;
		}

		private string BucketWithNamePrefix(string bucket)
		{
			return $"{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_PREFIX}.{Name}.{bucket}";
		}
	}
}
