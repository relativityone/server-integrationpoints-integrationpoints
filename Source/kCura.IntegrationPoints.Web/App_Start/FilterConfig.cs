using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Mvc;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
		
		public static void RegisterWebAPIFilters(HttpConfiguration config)
		{
			config.Filters.Add(new LogApiExceptionFilterAttribute());
		}

		public static void AddExceptionLogger(HttpConfiguration config, IExceptionLogger exceptionLogger)
		{
			config.Services.Add(typeof(IExceptionLogger), exceptionLogger);
		}
	}
}