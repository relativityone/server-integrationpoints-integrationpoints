using System;

namespace Relativity.Sync.Telemetry
{
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class APMIgnoreMetricAttribute : Attribute
    {
    }
}
