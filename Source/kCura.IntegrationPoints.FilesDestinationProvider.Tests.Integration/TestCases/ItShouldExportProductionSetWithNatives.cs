using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using kCura.Utility.Extensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	public class ItShouldExportProductionSetWithNatives : ExportTestCaseBase
	{
		private readonly ConfigSettings _configSettings;

		public ItShouldExportProductionSetWithNatives(ConfigSettings configSettings)
		{
			_configSettings = configSettings;
		}

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ProductionId = _configSettings.ProductionArtifactId;
			settings.TypeOfExport = ExportSettings.ExportType.ProductionSet;
			settings.ExportNativesToFileNamedFrom = ExportSettings.NativeFilenameFromType.Identifier;

			settings.OutputDataFileFormat = ExportSettings.DataFileFormat.Concordance;

			settings.ExportNatives = true;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			var expectedDataFileName = $"{_configSettings.ProductionArtifactName}_export.dat";
			var dataFiles = directory.EnumerateFiles(expectedDataFileName);
			Assert.That(dataFiles.Any());

			var nativesRootDirectory = directory.EnumerateDirectories("NATIVES", SearchOption.AllDirectories).ToList();
			var imagesRootDirectory = directory.EnumerateDirectories("IMAGES", SearchOption.AllDirectories);

			// we don expect any images generated for this case
			Assert.That(imagesRootDirectory.IsNullOrEmpty());
			
			Assert.That(!nativesRootDirectory.IsNullOrEmpty());

			var actualFileCount = Directory.EnumerateFiles(nativesRootDirectory[0].FullName, "*", SearchOption.AllDirectories).Count();

			Assert.That(actualFileCount, Is.EqualTo(documentsTestData.AllDocumentsDataTable.Rows.Count));
		}
	}
}