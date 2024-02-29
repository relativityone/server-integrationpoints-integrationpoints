namespace Relativity.Sync.Telemetry.RelEye
{
    internal sealed class JobEndEvent : EventBase<JobEndEvent>
    {
        [RelEye(Const.Names.JobResult)]
        public ExecutionStatus Status { get; set; }

        public override string EventName => EventNames.JobEnd;
    }
}
