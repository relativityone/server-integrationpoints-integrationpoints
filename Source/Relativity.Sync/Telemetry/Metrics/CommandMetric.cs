namespace Relativity.Sync.Telemetry.Metrics
{
    internal sealed class CommandMetric : MetricBase<CommandMetric>
    {
        public ExecutionStatus? ExecutionStatus { get; set; }

        [Metric(MetricType.TimedOperation)]
        public double? Duration { get; set; }

        public CommandMetric()
        {
        }

        public CommandMetric(string commandName) : base(commandName)
        {
        }
    }
}
