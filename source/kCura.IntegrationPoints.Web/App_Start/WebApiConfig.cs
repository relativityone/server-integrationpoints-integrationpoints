using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace kCura.IntegrationPoints.Web
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			
			config.Routes.MapHttpRoute(
					name: "DefaultApi",
					routeTemplate: "{workspaceID}/api/{controller}/{id}",
					defaults: new { id = RouteParameter.Optional }
			);
			//config.MapHttpAttributeRoutes();
		}
	}
}
