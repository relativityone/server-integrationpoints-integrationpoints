using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Telemetry.Metrics
{
	public interface IMetricCollection
	{
		IMetricCollection AddMetric<T>(T metric) where T: IMetric;
		Task SendAsync();

	}
}
