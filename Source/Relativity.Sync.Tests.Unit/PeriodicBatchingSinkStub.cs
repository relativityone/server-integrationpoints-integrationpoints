using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Relativity.Telemetry.APM;
using Relativity.Telemetry.APM.Interfaces;
using Relativity.Telemetry.APM.Sinks;

namespace Relativity.Sync.Tests.Unit
{
    public class PeriodicBatchingSinkStub : PeriodicBatchingSink
    {
        public PeriodicBatchingSinkStub() : base(0, TimeSpan.Zero) { }

        public string ToJson(IDictionary<string, object> dictionary)
        {
            IMeasureResult measureResult = new CounterResult { CustomData = new ConcurrentDictionary<string, object>(dictionary) };
            return ToJson(measureResult);
        }
    }
}
