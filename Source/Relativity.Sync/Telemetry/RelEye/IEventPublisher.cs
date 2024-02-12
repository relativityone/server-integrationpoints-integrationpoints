namespace Relativity.Sync.Telemetry.RelEye
{
    internal interface IEventPublisher
    {
        void Publish(IEvent @event);
    }
}
