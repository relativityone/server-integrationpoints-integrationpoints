using System;
using System.Web.Mvc;
using Castle.Windsor;
using kCura.IntegrationPoints.Web.Metrics;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Attributes
{
    public class MvcActionExecutionTimeMetricsFilterAttribute : System.Web.Mvc.ActionFilterAttribute
    {
        private const string _TIMESTAMP_KEY_NAME = "MvcActionExecutionStartTimestamp";

        public IWindsorContainer Container { get; set; } = WindsorServiceLocator.Container;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            IAPILog logger = GetService<IAPILog>();

            if (filterContext.HttpContext.Items.Contains(_TIMESTAMP_KEY_NAME))
            {
                logger.LogWarning("HttpContext.Items dictionary already contains '{key}' key.", _TIMESTAMP_KEY_NAME);
                return;
            }

            filterContext.HttpContext.Items.Add(_TIMESTAMP_KEY_NAME, GetService<IDateTimeHelper>().Now());

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

            GetService<IControllerActionExecutionTimeMetrics>().LogExecutionTime(
                FormatURL(filterContext.HttpContext.Request.RawUrl), 
                (DateTime)filterContext.HttpContext.Items[_TIMESTAMP_KEY_NAME],
                filterContext.HttpContext.Request.HttpMethod);
        }

        /// <summary>
        /// Truncates query string from the raw url.
        /// </summary>
        private string FormatURL(string url)
        {
            string cleanUrl = url.Split('?')[0];
            return cleanUrl;
        }

        private T GetService<T>()
        {
            return Container.Resolve<T>();
        }
    }
}