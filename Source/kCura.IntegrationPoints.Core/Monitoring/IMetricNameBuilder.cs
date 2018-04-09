using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Monitoring
{
	public interface IMetricNameBuilder
	{
		string Build(string template, string provider);
	}
}
