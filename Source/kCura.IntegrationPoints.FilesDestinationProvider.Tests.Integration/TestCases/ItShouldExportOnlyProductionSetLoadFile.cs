using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportOnlyProductionSetLoadFile : ExportTestCaseBase
	{
		private readonly ConfigSettings _configSettings;

		public ItShouldExportOnlyProductionSetLoadFile(ConfigSettings configSettings)
		{
			_configSettings = configSettings;
		}

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ProductionId = _configSettings.ProductionArtifactId;
			settings.TypeOfExport = ExportSettings.ExportType.ProductionSet;
			settings.ExportNativesToFileNamedFrom = ExportSettings.NativeFilenameFromType.Identifier;

			settings.OutputDataFileFormat = ExportSettings.DataFileFormat.Concordance;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			var expectedDataFileName = $"{_configSettings.ProductionArtifactName}_export.dat";
			var dataFiles = directory.EnumerateFiles(expectedDataFileName);
			Assert.That(dataFiles.Any());

			var nativesRootDirectory = directory.EnumerateDirectories("NATIVES", SearchOption.AllDirectories).ToList();
			var imagesRootDirectory = directory.EnumerateDirectories("IMAGES", SearchOption.AllDirectories).ToList();

			Assert.That(nativesRootDirectory.Count, Is.EqualTo(0));
			Assert.That(imagesRootDirectory.Count, Is.EqualTo(0));
		}
	}
}