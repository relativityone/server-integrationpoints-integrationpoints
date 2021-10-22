using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Common.Metrics;

namespace kCura.IntegrationPoints.Web.Metrics
{
	public class ControllerActionExecutionTimeMetrics : IControllerActionExecutionTimeMetrics
	{
		private readonly IDateTimeHelper _dateTimeHelper;
		private readonly IRipMetrics _ripMetrics;

		public ControllerActionExecutionTimeMetrics(IDateTimeHelper dateTimeHelper, IRipMetrics ripMetrics)
		{
			_dateTimeHelper = dateTimeHelper;
			_ripMetrics = ripMetrics;
		}

		public void LogExecutionTime(string url, DateTime startTime, string method)
		{
			TimeSpan duration = _dateTimeHelper.Now() - startTime;
			long responseTimeInMs = (long)duration.TotalMilliseconds;

            _ripMetrics.TimedOperation(Core.Constants.IntegrationPoints.Telemetry.BUCKET_INTEGRATION_POINT_CUSTOMPAGE_RESPONSE_TIME, duration, new Dictionary<string, object>()
            {
                { "ActionURL", url },
                { "ResponseTimeMs", responseTimeInMs },
                { "Method", method }
            });
        }
	}
}