namespace Relativity.Sync.Telemetry.RelEye
{
    internal sealed class JobStartEvent : EventBase<JobStartEvent>
    {
        [RelEye(Const.Names.JobType)]
        public string Type { get; set; }

        public override string EventName => EventNames.JobStart;
    }
}
