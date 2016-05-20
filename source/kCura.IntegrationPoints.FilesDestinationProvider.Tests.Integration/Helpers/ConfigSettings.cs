using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	internal class ConfigSettings
	{
		#region Fields

		private const string _Workspace_Name_Key = "WorkspaceName";
		private const string _SavedSearch_Artifact_Name_Key = "SavedSearchArtifactName";
		private const string _Selected_Field_Names_Key = "SelectedFieldNames";
		private const string _Destination_Path_Key = "DestinationPath";
		private const string _WebApi_Url_Key = "WebApiUrl";

		#endregion //Fields

		#region Properties

		public string WorkspaceName { get; } = ConfigurationManager.AppSettings[_Workspace_Name_Key];

		public string SavedSearchArtifactName { get; } = ConfigurationManager.AppSettings[_SavedSearch_Artifact_Name_Key];

		public List<string> SelectedFieldNames { get; } = ConfigurationManager.AppSettings[_Selected_Field_Names_Key]
			.Split(',')
			.Select(name => name.Trim())
			.ToList();

		public string DestinationPath { get; } = ConfigurationManager.AppSettings[_Destination_Path_Key];

		public string WebApiUrl { get; } = ConfigurationManager.AppSettings[_WebApi_Url_Key];

		#endregion //Properties
	}

}
