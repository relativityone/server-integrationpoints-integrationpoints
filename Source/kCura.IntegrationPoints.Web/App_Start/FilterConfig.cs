using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Mvc;
using Castle.Windsor;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web
{
	public static class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
		
		public static void RegisterWebAPIFilters(HttpConfiguration config, IWindsorContainer container)
		{
			config.Filters.Add(new LogApiExceptionFilterAttribute());
			config.Services.Add(typeof(IExceptionLogger), container.Resolve<WebAPIFilterException>());
		}

	}
}