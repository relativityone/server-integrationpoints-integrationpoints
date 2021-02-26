﻿namespace Relativity.Sync.Telemetry.Metrics
{
	internal class ValidationMetric : MetricBase<ValidationMetric>
	{
		public ExecutionStatus? ExecutionStatus { get; set; }

		[Metric(MetricType.TimedOperation)]
		public double? Duration { get; set; }

		[Metric(MetricType.Counter)]
		public Counter? FailedCounter { get; set; }

		public ValidationMetric(string validatorName) : base(validatorName)
		{
		}
	}
}
