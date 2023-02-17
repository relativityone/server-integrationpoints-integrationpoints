using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Castle.Windsor;
using kCura.IntegrationPoints.Web.Metrics;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Attributes
{
    public class ApiActionExecutionTimeMetricsFilterAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {
        private const string _TIMESTAMP_KEY_NAME = "ApiActionExecutionStartTimestamp";

        public IWindsorContainer Container { get; set; } = WindsorServiceLocator.Container;

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            IAPILog logger = GetService<IAPILog>();

            if (actionContext.Request.Properties.ContainsKey(_TIMESTAMP_KEY_NAME))
            {
                logger.LogWarning("Request.Properties dictionary already contains '{key}' key.", _TIMESTAMP_KEY_NAME);
                return;
            }

            actionContext.Request.Properties.Add(_TIMESTAMP_KEY_NAME, GetService<IDateTimeHelper>().Now());

            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);

            IAPILog logger = GetService<IAPILog>();

            if (!actionExecutedContext.Request.Properties.ContainsKey(_TIMESTAMP_KEY_NAME))
            {
                logger.LogWarning("Request.Properties dictionary does not contain '{key}' key.", _TIMESTAMP_KEY_NAME);
                return;
            }

            GetService<IControllerActionExecutionTimeMetrics>().LogExecutionTime(
                FormatURL(actionExecutedContext.Request.RequestUri.AbsolutePath),
                (DateTime)actionExecutedContext.Request.Properties[_TIMESTAMP_KEY_NAME],
                actionExecutedContext.Request.Method.Method);
        }

        /// <summary>
        /// Removes protocol name, host name, and application path from the API URL.
        /// </summary>
        private string FormatURL(string url)
        {
            int startIndex = url.IndexOf("/api/", StringComparison.InvariantCultureIgnoreCase);
            return url.Substring(startIndex);
        }

        private T GetService<T>()
        {
            return Container.Resolve<T>();
        }
    }
}
