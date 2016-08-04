using System.Configuration;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	internal class ConfigSettings
	{
		#region Fields

		private const string _SAVEDSEARCH_ARTIFACT_NAME_KEY = "SavedSearchArtifactName";
		private const string _ADDITIONAL_FIELD_NAMES_KEY = "AdditionalFieldNames";
		private const string _DESTINATION_PATH_KEY = "DestinationPath";
		private const string _WEBAPI_URL_KEY = "WebApiUrl";

		private const string _USERNAME_KEY = "relativityUserName";
		private const string _PASSWORD_KEY = "relativityPassword";

		#endregion //Fields

		#region Properties

		public int WorkspaceId { get; set; }

		public int ExportedObjArtifactId { get; set; }

		public FieldEntry[] DefaultFields { get; set; }

		public FieldEntry[] AdditionalFields { get; set; }

		public string WorkspaceName { get; set; }

		public string SavedSearchArtifactName { get; } = ConfigurationManager.AppSettings[_SAVEDSEARCH_ARTIFACT_NAME_KEY];

		public string[] AdditionalFieldNames { get; } = ConfigurationManager.AppSettings[_ADDITIONAL_FIELD_NAMES_KEY]
			.Split(',')
			.Select(name => name.Trim())
			.ToArray();

		public string DestinationPath { get; } = ConfigurationManager.AppSettings[_DESTINATION_PATH_KEY];

		public string WebApiUrl { get; } = ConfigurationManager.AppSettings[_WEBAPI_URL_KEY];

		public string UserName { get; } = ConfigurationManager.AppSettings[_USERNAME_KEY];

		public string Password { get; } = ConfigurationManager.AppSettings[_PASSWORD_KEY];

		#endregion //Properties
	}
}