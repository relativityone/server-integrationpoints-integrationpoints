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
				defaults: new {controller = "FtpProviderAPI", action = "Decrypt"}
			);

			config.Routes.MapHttpRoute(
				name: "FTPEncrypt",
				routeTemplate: "{workspaceID}/api/FtpProviderAPI/e",
				defaults: new {controller = "FtpProviderAPI", action = "Encrypt"}
			);

			config.Routes.MapHttpRoute(
				name: "FTPGetColumnList",
				routeTemplate: "{workspaceID}/api/FtpProviderAPI/r",
				defaults: new {controller = "FtpProviderAPI", action = "GetColumnList"}
			);

			config.Routes.MapHttpRoute(
				name: "FTPViewSettings",
				routeTemplate: "{workspaceID}/api/FtpProviderAPI/view",
				defaults: new {controller = "FtpProviderAPI", action = "GetViewFields"}
			);

			config.Routes.MapHttpRoute(
				name: "LDAPViewSettings",
				routeTemplate: "{workspaceID}/api/ldap/view",
				defaults: new {controller = "ldap", action = "GetViewFields"}
			);

			config.Routes.MapHttpRoute(
				name: "LDAPCheckLdap",
				routeTemplate: "{workspaceID}/api/ldap/checkLdap",
				defaults: new { controller = "ldap", action = "CheckLdap" }
			);

			config.Routes.MapHttpRoute(
				name: "Save",
				routeTemplate: "{workspaceID}/api/IntegrationPointProfilesAPI",
				defaults: new {controller = "IntegrationPointProfilesAPI", action = "Save"}
			);

			config.Routes.MapHttpRoute(
				name: "SaveProfileUsingIP",
				routeTemplate:
				"{workspaceID}/api/IntegrationPointProfilesAPI/SaveAsProfile",
				defaults: new {controller = "IntegrationPointProfilesAPI", action = "SaveUsingIntegrationPoint"}
			);

			config.Routes.MapHttpRoute(
				name: "GetProfile",
				routeTemplate: "{workspaceID}/api/IntegrationPointProfilesAPI/{artifactId}",
				defaults: new {controller = "IntegrationPointProfilesAPI", action = "Get"},
				constraints: new {artifactId = @"^[0-9]+$"}
			);

			config.Routes.MapHttpRoute(
				name: "GetAllProfiles",
				routeTemplate: "{workspaceID}/api/IntegrationPointProfilesAPI/GetAll",
				defaults: new {controller = "IntegrationPointProfilesAPI", action = "GetAll"}
			);

			config.Routes.MapHttpRoute(
				name: "GetProfilesByType",
				routeTemplate: "{workspaceID}/api/IntegrationPointProfilesAPI/GetByType/{artifactId}",
				defaults: new {controller = "IntegrationPointProfilesAPI", action = "GetByType"}
			);

			config.Routes.MapHttpRoute(
				name: "GetValidatedProfileModel",
				routeTemplate: "{workspaceID}/api/IntegrationPointProfilesAPI/GetValidatedProfileModel/{artifactId}",
				defaults: new {controller = "IntegrationPointProfilesAPI", action = "GetValidatedProfileModel"}
			);

			config.Routes.MapHttpRoute(
				name: "RelativityViewSettings",
				routeTemplate: "{workspaceID}/api/relativity/view",
				defaults: new {controller = "relativity", action = "GetViewFields"}
			);

			config.Routes.MapHttpRoute(
				name: "FolderPathGetFields",
				routeTemplate: "{workspaceID}/api/FolderPath/GetFields",
				defaults: new {controller = "FolderPath", action = "GetFields"}
			);

			config.Routes.MapHttpRoute(
				name: "FolderPathGetLongTextFields",
				routeTemplate: "{workspaceID}/api/FolderPath/GetLongTextFields",
				defaults: new {controller = "FolderPath", action = "GetLongTextFields"}
			);

			config.Routes.MapHttpRoute(
				name: "FolderPathGetChoiceFields",
				routeTemplate: "{workspaceID}/api/FolderPath/GetChoiceFields",
				defaults: new {controller = "FolderPath", action = "GetChoiceFields"}
			);

			config.Routes.MapHttpRoute(
				name: "FolderPathGetFolderCount",
				routeTemplate: "{workspaceID}/api/FolderPath/GetFolderCount/{integrationPointArtifactId}",
				defaults:
				new {controller = "FolderPath", action = "GetFolderCount", integrationPointArtifactId = RouteParameter.Optional}
			);

			config.Routes.MapHttpRoute(
				name: "JobRun",
				routeTemplate: "{workspaceID}/api/Job",
				defaults: new {controller = "Job", action = "Run"}
			);

			config.Routes.MapHttpRoute(
				name: "JobStop",
				routeTemplate: "{workspaceID}/api/Job/Stop",
				defaults: new {controller = "Job", action = "Stop"}
			);

			config.Routes.MapHttpRoute(
				name: "JobRetry",
				routeTemplate: "{workspaceID}/api/Job/Retry",
				defaults: new {controller = "Job", action = "Retry"}
			);

			config.Routes.MapHttpRoute(
				name: "ProductionPrecedence",
				routeTemplate: "{workspaceID}/api/ProductionPrecedence/ProductionPrecedence",
				defaults: new {controller = "ProductionPrecedence", action = "GetProductionPrecedence"}
			);

			config.Routes.MapHttpRoute(
				name: "LongTextFields",
				routeTemplate: "{workspaceID}/api/ExportFields/LongTextFields/{artifactTypeId}",
				defaults: new {controller = "ExportFields", action = "GetExportableLongTextFields"}
			);

			config.Routes.MapHttpRoute(
				name: "ExportableFields",
				routeTemplate: "{workspaceID}/api/ExportFields/Exportable",
				defaults: new {controller = "ExportFields", action = "GetExportableFields"}
			);

			config.Routes.MapHttpRoute(
				name: "AvailableFields",
				routeTemplate: "{workspaceID}/api/ExportFields/Available",
				defaults: new {controller = "ExportFields", action = "GetAvailableFields"}
			);

			config.Routes.MapHttpRoute(
				name: "ValidateSettings",
				routeTemplate: "{workspaceID}/api/ExportSettingsValidation/Validate",
				defaults: new {controller = "ExportSettingsValidation", action = "ValidateSettings"}
			);

			config.Routes.MapHttpRoute(
				name: "GetProcessingSourceLocations",
				routeTemplate: "{workspaceID}/api/ResourcePool/GetProcessingSourceLocations",
				defaults: new {controller = "ResourcePool", action = "GetProcessingSourceLocations"}
			);

			config.Routes.MapHttpRoute(
				name: "GetProcessingSourceLocationSubItems",
				routeTemplate: "{workspaceID}/api/ResourcePool/GetProcessingSourceLocationSubItems/{isRoot}",
				defaults: new {controller = "ResourcePool", action = "GetSubItems"}
			);

			config.Routes.MapHttpRoute(
				name: "IsProcessingSourceLocationEnabled",
				routeTemplate: "{workspaceID}/api/ResourcePool/IsProcessingSourceLocationEnabled",
				defaults: new {controller = "ResourcePool", action = "IsProcessingSourceLocationEnabled"}
			);

			config.Routes.MapHttpRoute(
				name: "SavedSearchesTree",
				routeTemplate: "{workspaceID}/api/SavedSearchesTree/{workspaceArtifactId}",
				defaults: new {controller = "SavedSearchesTree", action = "Get"}
			);

			config.Routes.MapHttpRoute(
				name: "GetStructure",
				routeTemplate: "{workspaceID}/api/DataTransferLocation/GetStructure/{integrationPointTypeIdentifier}",
				defaults: new {controller = "DataTransferLocation", action = "GetStructure"}
			);

			config.Routes.MapHttpRoute(
				name: "GetRoot",
				routeTemplate: "{workspaceID}/api/DataTransferLocation/GetRoot/{integrationPointTypeIdentifier}",
				defaults: new {controller = "DataTransferLocation", action = "GetRoot"}
			);

			config.Routes.MapHttpRoute(
				name: "GetViewsByWorkspaceAndArtifactType",
				routeTemplate: "{workspaceID}/api/WorkspaceView/GetViews/{artifactTypeId}",
				defaults: new {controller = "WorkspaceView", action = "GetViewsByWorkspaceAndArtifactType"}
			);

			config.Routes.MapHttpRoute(
				name: "ImportPreviewFiles",
				routeTemplate: "{workspaceID}/api/ImportPreview/CreatePreviewJob/",
				defaults: new {controller = "ImportPreview", action = "CreatePreviewJob"}
			);

			config.Routes.MapHttpRoute(
				name: "ImportPreviewProgress",
				routeTemplate: "{workspaceID}/api/ImportPreview/CheckProgress/{jobId}",
				defaults: new {controller = "ImportPreview", action = "CheckProgress"}
			);

			config.Routes.MapHttpRoute(
				name: "ImportPreviewTable",
				routeTemplate: "{workspaceID}/api/ImportPreview/GetImportPreviewTable/{jobId}",
				defaults: new {controller = "ImportPreview", action = "GetImportPreviewTable"}
			);

			config.Routes.MapHttpRoute(
				name: "AsciiDelimiters",
				routeTemplate: "{workspaceID}/api/ImportProviderDocument/GetAsciiDelimiters",
				defaults: new {controller = "ImportProviderDocument", action = "GetAsciiDelimiters"}
			);

			config.Routes.MapHttpRoute(
				name: "GetMappableFieldsFromSourceWorkspace",
				routeTemplate: "{workspaceID}/api/FieldMappings/GetMappableFieldsFromSourceWorkspace",
				defaults: new { controller = "FieldMappings", action = "GetMappableFieldsFromSourceWorkspace" }
			);

			config.Routes.MapHttpRoute(
				name: "GetMappableFieldsFromDestinationWorkspace",
				routeTemplate: "{workspaceID}/api/FieldMappings/GetMappableFieldsFromDestinationWorkspace",
				defaults: new { controller = "FieldMappings", action = "GetMappableFieldsFromDestinationWorkspace" }
			);

			config.Routes.MapHttpRoute(
				name: "AutoMapFields",
				routeTemplate: "{workspaceID}/api/FieldMappings/AutomapFields/{destinationProviderGuid}",
				defaults: new { controller = "FieldMappings", action = "AutoMapFields" }
			);

			config.Routes.MapHttpRoute(
				name: "ValidateFieldsMapping",
				routeTemplate: "{workspaceID}/api/FieldMappings/Validate/{destinationWorkspaceID}/{destinationProviderGuid}",
				defaults: new { controller = "FieldMappings", action = "ValidateAsync" }
			);
			
			config.Routes.MapHttpRoute(
				name: "AutoMapFieldsFromSavedSearch",
				routeTemplate: "{sourceWorkspaceID}/api/FieldMappings/AutomapFieldsFromSavedSearch/{savedSearchID}/{destinationProviderGuid}",
				defaults: new { controller = "FieldMappings", action = "AutoMapFieldsFromSavedSearch" }
			);

			config.Routes.MapHttpRoute(
				name: "LoadFileHeaders",
				routeTemplate: "{workspaceID}/api/ImportProviderDocument/LoadFileHeaders",
				defaults: new {controller = "ImportProviderDocument", action = "LoadFileHeaders"}
			);

			config.Routes.MapHttpRoute(
				name: "ImportProviderDocumentGetImportTypes",
				routeTemplate: "{workspaceID}/api/ImportProviderDocument/GetImportTypes",
				defaults: new {controller = "ImportProviderDocument", action = "GetImportTypes"}
			);

			config.Routes.MapHttpRoute(
				name: "ImportProviderDocumentIsCloudInstance",
				routeTemplate: "{workspaceID}/api/ImportProviderDocument/IsCloudInstance",
				defaults: new {controller = "ImportProviderDocument", action = "IsCloudInstance"}
			);

			config.Routes.MapHttpRoute(
				name: "ImportProviderDocumentViewData",
				routeTemplate: "{workspaceID}/api/ImportProviderDocument/ViewData",
				defaults: new {controller = "ImportProviderDocument", action = "ViewData"}
			);

			config.Routes.MapHttpRoute(
				name: "ImportProviderDocumentDownloadErrorFile",
				routeTemplate: "{workspaceID}/api/ImportProviderDocument/DownloadErrorFile",
				defaults: new {controller = "ImportProviderDocument", action = "DownloadErrorFile"}
			);

			config.Routes.MapHttpRoute(
				name: "ImportProviderDocumentCheckErrorFile",
				routeTemplate: "{workspaceID}/api/ImportProviderDocument/CheckErrorFile",
				defaults: new {controller = "ImportProviderDocument", action = "CheckErrorFile"}
			);

			config.Routes.MapHttpRoute(
				name: "ErrorGetViewErrorsLink",
				routeTemplate: "{workspaceID}/api/ButtonState/GetUserPermissionsCheck",
				defaults: new { controller = "ButtonState", action = "GetUserPermissionsCheck" }
			);

			config.Routes.MapHttpRoute(
				name: "ButtonStateGetUserPermissionsCheck",
				routeTemplate: "{workspaceID}/api/Error/GetViewErrorsLink",
				defaults: new { controller = "Error", action = "GetViewErrorsLink" }
			);

			config.Routes.MapHttpRoute(
				name: "ImportProviderGetOverlayIdentifierFields",
				routeTemplate: "{workspaceID}/api/ImportProviderImage/GetOverlayIdentifierFields",
				defaults: new {controller = "ImportProviderImage", action = "GetOverlayIdentifierFields"}
			);

			config.Routes.MapHttpRoute(
				name: "ImportProviderGetFileRepositories",
				routeTemplate: "{workspaceID}/api/ImportProviderImage/GetFileRepositories",
				defaults: new {controller = "ImportProviderImage", action = "GetFileRepositories"}
			);

			config.Routes.MapHttpRoute(
				name: "ImportProviderGetDefaultFileRepo",
				routeTemplate: "{workspaceID}/api/ImportProviderImage/GetDefaultFileRepo",
				defaults: new {controller = "ImportProviderImage", action = "GetDefaultFileRepo"}
			);

			config.Routes.MapHttpRoute(
				name: "GetFolderFullPath",
				routeTemplate:
				"{workspaceID}/api/SearchFolder/GetFullPathList/{destinationWorkspaceId}/{folderArtifactId}/{federatedInstanceId}",
				defaults: new {controller = "SearchFolder", action = "GetFullPathList"}
			);

			config.Routes.MapHttpRoute(
				name: "GetFolderStructure",
				routeTemplate:
				"{workspaceID}/api/SearchFolder/GetStructure/{destinationWorkspaceId}/{folderArtifactId}/{federatedInstanceId}",
				defaults: new {controller = "SearchFolder", action = "GetStructure"}
			);

			config.Routes.MapHttpRoute(
				name: "GetDefaultRdoTypeId",
				routeTemplate: "{workspaceID}/api/RdoFilter/GetDefaultRdoTypeId",
				defaults: new {controller = "RdoFilter", action = "GetDefaultRdoTypeId"}
			);

			config.Routes.MapHttpRoute(
				name: "GetAllViewableRdos",
				routeTemplate: "{workspaceID}/api/RdoFilter/GetAll",
				defaults: new {controller = "RdoFilter", action = "GetAllViewableRdos"}
			);

			config.Routes.MapHttpRoute(
				name: "ProductionGetProductionsForImport",
				routeTemplate: "{workspaceID}/api/Production/GetProductionsForImport/{workspaceArtifactId}/{federatedInstanceId}",
				defaults:
				new {controller = "Production", action = "GetProductionsForImport", federatedInstanceId = RouteParameter.Optional}
			);

			config.Routes.MapHttpRoute(
				name: "ProductionGetProductionsForExport",
				routeTemplate: "{workspaceID}/api/Production/GetProductionsForExport",
				defaults: new {controller = "Production", action = "GetProductionsForExport"}
			);

			config.Routes.MapHttpRoute(
				name: "CreateProduction",
				routeTemplate:
				"{workspaceID}/api/Production/CreateProductionSet/{productionName}/{workspaceArtifactId}/{federatedInstanceId}",
				defaults:
				new {controller = "Production", action = "CreateProductionSet", federatedInstanceId = RouteParameter.Optional}
			);

			config.Routes.MapHttpRoute(
				name: "CheckProductionAddPermission",
				routeTemplate: "{workspaceID}/api/Production/CheckProductionAddPermission/{workspaceArtifactId}/{federatedInstanceId}",
				defaults:
				new {controller = "Production", action = "CheckProductionAddPermission", federatedInstanceId = RouteParameter.Optional}
			);

			config.Routes.MapHttpRoute(
				name: "GetCurrentInstanceWorkspaces",
				routeTemplate: "{workspaceID}/api/WorkspaceFinder",
				defaults: new {controller = "WorkspaceFinder", action = "GetCurrentInstanceWorkspaces"}
			);

			config.Routes.MapHttpRoute(
				name: "GetFederatedInstanceWorkspaces",
				routeTemplate: "{workspaceID}/api/WorkspaceFinder/{federatedInstanceId}",
				defaults: new {controller = "WorkspaceFinder", action = "GetFederatedInstanceWorkspaces"}
			);
			
			config.Routes.MapHttpRoute(
				name: "CheckToggle",
				routeTemplate: "{workspaceID}/api/ToggleAPI/{toggleName}",
				defaults: new {controller = "ToggleAPI", action = "Get"}
			);

			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "{workspaceID}/api/{controller}/{id}",
				defaults: new {id = RouteParameter.Optional}
			);
		}
	}
}