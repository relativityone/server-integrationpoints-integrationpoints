using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	internal class ConfigSettings
	{
		private const string WorkspaceNameKey = "WorkspaceName";
		private const string SavedSearchArtifactNameKey = "SavedSearchArtifactName";
		private const string SelectedFieldNamesKey = "SelectedFieldNames";
		private const string DestinationPathKey = "DestinationPath";
		private const string WebApiUrlKey = "WebApiUrl";

		public string WorkspaceName { get; } = ConfigurationManager.AppSettings[WorkspaceNameKey];

		public string SavedSearchArtifactName { get; } = ConfigurationManager.AppSettings[SavedSearchArtifactNameKey];

		public List<string> SelectedFieldNames { get; } = ConfigurationManager.AppSettings[SelectedFieldNamesKey]
			.Split(',')
			.Select(name => name.Trim())
			.ToList();

		public string DestinationPath { get; } = ConfigurationManager.AppSettings[DestinationPathKey];

		public string WebApiUrl { get; } = ConfigurationManager.AppSettings[WebApiUrlKey];
	}

}
