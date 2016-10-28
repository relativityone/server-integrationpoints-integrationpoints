﻿using System.Web.Http;

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
				name: "JobStop",
				routeTemplate: "{workspaceID}/api/Job/Stop",
				defaults: new { controller = "Job", action = "Stop" }
			);

			config.Routes.MapHttpRoute(
				name: "JobRetry",
				routeTemplate: "{workspaceID}/api/Job/Retry",
				defaults: new { controller = "Job", action = "Retry" }
			);

			config.Routes.MapHttpRoute(
				name: "ProductionPrecedence",
				routeTemplate: "{workspaceID}/api/ProductionPrecedence/ProductionPrecedence",
				defaults: new { controller = "ProductionPrecedence", action = "GetProductionPrecedence" }
			);

			config.Routes.MapHttpRoute(
				name: "LongTextFields",
				routeTemplate: "{workspaceID}/api/ExportFields/LongTextFields",
				defaults: new { controller = "ExportFields", action = "GetExportableLongTextFields" }
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
				name: "ValidateSettings",
				routeTemplate: "{workspaceID}/api/ExportSettingsValidation/Validate",
				defaults: new { controller = "ExportSettingsValidation", action = "ValidateSettings" }
			);

			config.Routes.MapHttpRoute(
				name: "GetProcessingSourceLocations",
				routeTemplate: "{workspaceID}/api/ResourcePool/GetProcessingSourceLocations",
				defaults: new { controller = "ResourcePool", action = "GetProcessingSourceLocations" }
			);

			config.Routes.MapHttpRoute(
				name: "GetProcessingSourceLocationStructure",
				routeTemplate: "{workspaceID}/api/ResourcePool/GetProcessingSourceLocationStructure/{artifactId}",
				defaults: new { controller = "ResourcePool", action = "GetProcessingSourceLocationStructure" }
			);

			config.Routes.MapHttpRoute(
				name: "SavedSearchesTree",
				routeTemplate: "{workspaceID}/api/SavedSearchesTree/{workspaceArtifactId}",
				defaults: new { controller = "SavedSearchesTree", action = "Get" }
			);

			config.Routes.MapHttpRoute(
				name: "GetProcessingSourceLocationSubItems",
				routeTemplate: "{workspaceID}/api/ResourcePool/GetProcessingSourceLocationSubItems/{isRoot}",
				defaults: new { controller = "ResourcePool", action = "GetSubItems" }
			);

			config.Routes.MapHttpRoute(
				name: "GetViewsByWorkspaceAndArtifactType",
				routeTemplate: "{workspaceID}/api/WorkspaceView/GetViews/{artifactTypeId}",
				defaults: new { controller = "WorkspaceView", action = "GetViewsByWorkspaceAndArtifactType" }
			);

			config.Routes.MapHttpRoute(
				name: "ImportPreviewFiles",
				routeTemplate: "api/ImportPreview/CreatePreviewJob/",
				defaults: new { controller = "ImportPreview", action = "CreatePreviewJob" }
			);

			config.Routes.MapHttpRoute(
				name: "ImportPreviewProgress",
				routeTemplate: "api/ImportPreview/CheckProgress/{jobId}",
				defaults: new { controller = "ImportPreview", action = "CheckProgress" }
			);

			config.Routes.MapHttpRoute(
				name: "ImportPreviewTable",
				routeTemplate: "api/ImportPreview/GetImportPreviewTable/{jobId}",
				defaults: new { controller = "ImportPreview", action = "GetImportPreviewTable" }
			);

			config.Routes.MapHttpRoute(
				name: "AsciiDelimiters",
				routeTemplate: "api/ImportProviderDocument/GetAsciiDelimiters",
				defaults: new { controller = "ImportProviderDocument", action = "GetAsciiDelimiters" }
			);

			config.Routes.MapHttpRoute(
				name: "LoadFileHeaders",
				routeTemplate: "api/ImportProviderDocument/LoadFileHeaders",
				defaults: new { controller = "ImportProviderDocument", action = "LoadFileHeaders" }
			);

			config.Routes.MapHttpRoute(
			   name: "SearchFolder",
			   routeTemplate: "{workspaceID}/api/SearchFolder/{destinationWorkspaceId}",
			   defaults: new { controller = "SearchFolder", action = "Get" }
		   );

			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "{workspaceID}/api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);
		}
	}
}