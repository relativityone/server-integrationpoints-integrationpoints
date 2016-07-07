using System.Web.Http;

namespace kCura.IntegrationPoints.Web
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			config.Routes.MapHttpRoute(
				name: "FTPDecrypt",
				routeTemplate: "{workspaceID}/api/FtpProviderAPI/d",
				defaults: new { controller = "FtpProviderAPI", action = "Decrypt" }
			);

			config.Routes.MapHttpRoute(
				name: "FTPEncrypt",
				routeTemplate: "{workspaceID}/api/FtpProviderAPI/e",
				defaults: new { controller = "FtpProviderAPI", action = "Encrypt" }
			);

			config.Routes.MapHttpRoute(
				name: "FTPGetColumnList",
				routeTemplate: "{workspaceID}/api/FtpProviderAPI/r",
				defaults: new { controller = "FtpProviderAPI", action = "GetColumnList" }
			);

			config.Routes.MapHttpRoute(
				name: "FTPViewSettings",
				routeTemplate: "{workspaceID}/api/FtpProviderAPI/view",
				defaults: new { controller = "FtpProviderAPI", action = "GetViewFields" }
			);

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
				name: "JobRun",
				routeTemplate: "{workspaceID}/api/Job",
				defaults: new { controller = "Job", action = "Run" }
			);

			config.Routes.MapHttpRoute(
				name: "JobRetry",
				routeTemplate: "{workspaceID}/api/Job/Retry",
				defaults: new { controller = "Job", action = "Retry" }
			);

			config.Routes.MapHttpRoute(
				name: "ExportableFields",
				routeTemplate: "{workspaceID}/api/ExportFields/Exportable",
				defaults: new { controller = "ExportFields", action = "GetExportableFields" }
			);

			config.Routes.MapHttpRoute(
				name: "AvailableFields",
				routeTemplate: "{workspaceID}/api/ExportFields/Available",
				defaults: new { controller = "ExportFields", action = "GetAvailableFields" }
			);

			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "{workspaceID}/api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);
		}
	}
}