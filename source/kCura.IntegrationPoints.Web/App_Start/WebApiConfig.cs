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
		name: "LDAPdecrypt",
		routeTemplate: "{workspaceID}/api/ldap/d",
		defaults: new { controller = "ldap", action = "Decrypt" }
);
			config.Routes.MapHttpRoute(
name: "LDAPEncrypt",
routeTemplate: "{workspaceID}/api/ldap/e",
defaults: new { controller = "ldap", action = "Encrypt" }
);

			config.Routes.MapHttpRoute(
				name: "LDAPViewSettings",
				routeTemplate: "{workspaceID}/api/ldap/view",
				defaults: new { controller = "ldap", action = "GetViewFields" }
		);


			config.Routes.MapHttpRoute(
					name: "DefaultApi",
					routeTemplate: "{workspaceID}/api/{controller}/{id}",
					defaults: new { id = RouteParameter.Optional }
			);
			
			//config.MapHttpAttributeRoutes();
		}
	}
}
