using System;
using System.Configuration;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Process.Internals
{
	public class ExportTestConfiguration
	{
		private const string _SAVEDSEARCH_ARTIFACT_NAME_KEY = "Test.Elf.SavedSearchArtifactName";
		private const string _PRODUCTION_ARTIFACT_NAME_KEY = "Test.Elf.ProductionArtifactName";
		private const string _VIEW_ARTIFACT_NAME_KEY = "Test.Elf.ViewArtifactName";
		private const string _DESTINATION_PATH_KEY = "Test.Elf.DestinationPath";
		private const string _LONGT_TEXT_FIELD_KEY = "Test.Elf.LongTextFieldName";

		public string JobName => "ElfIntergationTest";

		public DateTime JobStart => new DateTime(2019, 5, 9, 4, 9, 53);

		public string SavedSearchArtifactName { get; } = ConfigurationManager.AppSettings[_SAVEDSEARCH_ARTIFACT_NAME_KEY];

		public string ProductionArtifactName { get; } = ConfigurationManager.AppSettings[_PRODUCTION_ARTIFACT_NAME_KEY];

		public string ViewName { get; } = ConfigurationManager.AppSettings[_VIEW_ARTIFACT_NAME_KEY];

		public string LongTextFieldName { get; } = ConfigurationManager.AppSettings[_LONGT_TEXT_FIELD_KEY];

		public string DestinationPath { get; } = ConfigurationManager.AppSettings[_DESTINATION_PATH_KEY];
	}
}
