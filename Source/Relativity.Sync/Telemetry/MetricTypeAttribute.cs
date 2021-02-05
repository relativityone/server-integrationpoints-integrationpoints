using System;

namespace Relativity.Sync.Telemetry
{
	[AttributeUsage(AttributeTargets.Property)]
	internal class MetricAttribute : Attribute
	{
		public MetricType Type { get; set; }

		public MetricAttribute(MetricType type, string name)
		{
			Type = type;
		}
	}
}
