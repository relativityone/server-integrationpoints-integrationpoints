using System.Collections.Generic;

namespace Relativity.Sync.Telemetry
{
    internal abstract class SyncMetricsSinkBase
    {
        public ISyncMetricsSink Sink { get; set; }
    }
}
