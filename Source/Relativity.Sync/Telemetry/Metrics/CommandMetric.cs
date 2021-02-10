namespace Relativity.Sync.Telemetry.Metrics
{
	internal class CommandMetric : MetricBase
	{
		public ExecutionStatus? ExecutionStatus { get; set; }

		[Metric(MetricType.TimedOperation)]
		public double? Duration { get; set; }

		public CommandMetric(string commandName) : base(commandName)
		{
		}


	}
}
