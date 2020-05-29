using Relativity.Telemetry.Services.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Telemetry.Metrics
{
	public interface IMetric
	{
		bool CanSend();
		Task SendAsync(IMetricsManager metrics);
	}
}
