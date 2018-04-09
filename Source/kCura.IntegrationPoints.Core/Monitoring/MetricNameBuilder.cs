using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Monitoring
{
	class MetricNameBuilder : IMetricNameBuilder
	{
		public string Build(string template, string provider)
		{
			return string.Format(template, provider);
		}
	}
}
