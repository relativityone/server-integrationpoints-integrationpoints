using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Castle.Windsor;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}

		public static void RegisterWebAPIFilters(HttpConfiguration config, IWindsorContainer container)
		{
			config.Services.Add(typeof(IExceptionFilter), container.Resolve<WebAPIFilterException>());
		}

	}
}