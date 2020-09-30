using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;
using System.Web.Mvc;
using Castle.Windsor;
using Relativity.API;
using Relativity.CustomPages;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.Web.Attributes
{

	//public class ExecutionTimeMetricsApiActionFilterAttribute : System.Web.Http.Filters.ActionFilterAttribute
	//{
	//	public override void OnActionExecuting(HttpActionContext actionContext)
	//	{
	//		actionContext.Request.Properties;
	//		base.OnActionExecuting(actionContext);
	//	}

	//	public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
	//	{
	//		base.OnActionExecuted(actionExecutedContext);
	//	}
	//}

	public class ExecutionTimeMetricsMvcActionFilterAttribute : System.Web.Mvc.ActionFilterAttribute
	{
		/// <summary>
		/// ASP.NET MVC will cache ActionFilters objects and try to reuse them on subsequent requests,
		/// so we cannot store DateTime object as private class field. Instead, we are using HttpContext.Items
		/// dictionary to store per-request object.
		/// </summary>
		private const string _TIMESTAMP_KEY_NAME = "ActionExecutionStartTimestamp";

		public IWindsorContainer Container { get; set; } = WindsorServiceLocator.Container;

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			IAPILog logger = GetService<IAPILog>();

			if (filterContext.HttpContext.Items.Contains(_TIMESTAMP_KEY_NAME))
			{
				logger.LogWarning("HttpContext.Items dictionary already contains '{key}' key.", _TIMESTAMP_KEY_NAME);
				return;
			}

			filterContext.HttpContext.Items.Add(_TIMESTAMP_KEY_NAME, DateTime.UtcNow);

			base.OnActionExecuting(filterContext);
		}

		public override void OnResultExecuted(ResultExecutedContext filterContext)
		{
			base.OnResultExecuted(filterContext);

			IAPILog logger = GetService<IAPILog>();

			if (!filterContext.HttpContext.Items.Contains(_TIMESTAMP_KEY_NAME))
			{
				logger.LogWarning("HttpContext.Items dictionary does not contain '{key}' key.", _TIMESTAMP_KEY_NAME);
				return;
			}

			DateTime startTime = (DateTime) filterContext.HttpContext.Items[_TIMESTAMP_KEY_NAME];
			TimeSpan duration = DateTime.UtcNow - startTime;
			long responseTimeInMs = (long)duration.TotalMilliseconds;
			string url = GetRequestCleanURL(filterContext);

			logger.LogInformation("Controller action response time: {time}ms Action URL: {url}",
				responseTimeInMs, url);

			IAPM apm = GetService<IAPM>();
			ITimerMeasure timedOperation = apm.TimedOperation(
				Core.Constants.IntegrationPoints.Telemetry.BUCKET_INTEGRATION_POINT_CUSTOMPAGE_RESPONSE_TIME,
				customData: new Dictionary<string, object>()
				{
					{ "ActionURL", url },
					{ "ResponseTimeMs", responseTimeInMs }
				});
		}

		/// <summary>
		/// Truncates query string from the RawUrl.
		/// </summary>
		private string GetRequestCleanURL(ControllerContext context)
		{
			string rawUrl = context.RequestContext.HttpContext.Request.RawUrl;
			string cleanUrl = rawUrl.Split('?')[0];
			return cleanUrl;
		}

		private T GetService<T>()
		{
			return Container.Resolve<T>();
		}
	}
}