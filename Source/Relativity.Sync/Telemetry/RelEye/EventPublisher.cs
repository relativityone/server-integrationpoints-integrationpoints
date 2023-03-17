using System.Collections.Generic;
using Relativity.API;

namespace Relativity.Sync.Telemetry.RelEye
{
    internal class EventPublisher : IEventPublisher
    {
        private readonly IAPMClient _apm;
        private readonly SyncJobParameters _params;
        private readonly IAPILog _log;

        public EventPublisher(IAPMClient apm, SyncJobParameters @params, IAPILog log)
        {
            _apm = apm;
            _params = @params;
            _log = log;
        }

        public void Publish(IEvent @event)
        {
            Dictionary<string, object> attrs = @event.GetValues();

            attrs[Const.Names.R1TeamID] = Const.Values.R1TeamID;
            attrs[Const.Names.ServiceName] = Const.Values.ServiceName;
            attrs[Const.Names.WorkflowId] = _params.WorkflowId;

            _log.LogInformation("Send Event: {@event}", attrs);

            _apm.Count(@event.EventName, attrs);
        }
    }
}
