using System.Collections.Generic;

namespace Relativity.Sync.Telemetry.RelEye
{
    internal interface IEvent
    {
        string EventName { get; }

        Dictionary<string, object> GetValues();
    }
}
