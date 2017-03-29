using System;
using System.Configuration;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Model;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	public class ConfigSettings
	{
		#region Fields

		private const string _SAVEDSEARCH_ARTIFACT_NAME_KEY = "SavedSearchArtifactName";
		private const string _PRODUCTION_ARTIFACT_NAME_KEY = "ProductionArtifactName";
		private const string _VIEW_ARTIFACT_NAME_KEY = "ViewArtifactName";
		private const string _ADDITIONAL_FIELD_NAMES_KEY = "AdditionalFieldNames";
		private const string _DESTINATION_PATH_KEY = "DestinationPath";

		private const string _LONGT_TEXT_FIELD_KEY = "LongTextFieldName";

		public static readonly string JobName = "IntergationTest";

		public static readonly DateTime JobStart = DateTime.UtcNow;

		#endregion //Fields

		#region Properties

		public int WorkspaceId { get; set; }

		public int ViewId { get; set; }

		public int ExportedObjArtifactId { get; set; }

		public int ProductionArtifactId { get; set; }

		public FieldEntry[] DefaultFields { get; set; }

		public FieldEntry[] AdditionalFields { get; set; }

		public FieldEntry LongTextField { get; set; }

		public string WorkspaceName { get; set; }

		public DocumentsTestData DocumentsTestData { get; set; }

		public string SavedSearchArtifactName { get; } = ConfigurationManager.AppSettings[_SAVEDSEARCH_ARTIFACT_NAME_KEY];

		public string ProductionArtifactName { get; } = ConfigurationManager.AppSettings[_PRODUCTION_ARTIFACT_NAME_KEY];

		public string ViewName { get; } = ConfigurationManager.AppSettings[_VIEW_ARTIFACT_NAME_KEY];

		public string[] AdditionalFieldNames { get; } = ConfigurationManager.AppSettings[_ADDITIONAL_FIELD_NAMES_KEY]
			.Split(',')
			.Select(name => name.Trim())
			.ToArray();

		public string LongTextFieldName { get; } = ConfigurationManager.AppSettings[_LONGT_TEXT_FIELD_KEY];

		public string DestinationPath { get; } = ConfigurationManager.AppSettings[_DESTINATION_PATH_KEY];

		#endregion //Properties
	}
}