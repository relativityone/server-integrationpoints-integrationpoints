using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Common.Metrics
{
	public interface IRipMetrics
	{
		void TimedOperation(string name, TimeSpan duration, Dictionary<string, object> customData);
	}
}