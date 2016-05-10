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
				name: "RelativityViewSettings",
				routeTemplate: "{workspaceID}/api/relativity/view",
				defaults: new { controller = "relativity", action = "GetViewFields" }
			);

			config.Routes.MapHttpRoute(
				name: "FolderPathGetFields",
				routeTemplate: "{workspaceID}/api/FolderPath/GetFields",
				defaults: new { controller = "FolderPath", action = "GetFields" }
			);

			config.Routes.MapHttpRoute(
				name: "FolderPathGetFolderCount",
				routeTemplate: "{workspaceID}/api/FolderPath/GetFolderCount/{integrationPointArtifactId}",
				defaults: new { controller = "FolderPath", action = "GetFolderCount", integrationPointArtifactId = RouteParameter.Optional }
			);

			config.Routes.MapHttpRoute(
				name: "ImportNowPost",
				routeTemplate: "{workspaceID}/api/ImportNow",
				defaults: new { controller = "ImportNow", action = "Post" }
			);

			config.Routes.MapHttpRoute(
				name: "ImportNowSubmitLastJob",
				routeTemplate: "{workspaceID}/api/ImportNow/SubmitLastJob",
				defaults: new { controller = "ImportNow", action = "SubmitLastJob" }
			);

			config.Routes.MapHttpRoute(
				name: "ImportNowRetryJob",
				routeTemplate: "{workspaceID}/api/ImportNow/RetryJob",
				defaults: new { controller = "ImportNow", action = "RetryJob" }
			);

			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "{workspaceID}/api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);
		}
	}
}
