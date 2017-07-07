using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.Core.Monitoring
{
	public static class HealthCheck
	{
		public static HealthCheckOperationResult CreateJobFailedMetric()
		{
			return new HealthCheckOperationResult("Integration Points job failed!");
		}
	}
}
