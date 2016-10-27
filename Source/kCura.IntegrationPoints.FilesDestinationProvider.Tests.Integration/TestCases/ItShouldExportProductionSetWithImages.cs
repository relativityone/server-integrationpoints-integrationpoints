using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Model;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using kCura.Utility.Extensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	class ItShouldExportProductionSetWithImages : ExportTestCaseBase
	{
		private readonly ConfigSettings _configSettings;

		public ItShouldExportProductionSetWithImages(ConfigSettings configSettings)
		{
			_configSettings = configSettings;
		}

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ProductionId = _configSettings.ProductionArtifactId;
			settings.ProductionName = "production__images_name";
			settings.TypeOfExport = ExportSettings.ExportType.ProductionSet;
			settings.ExportNativesToFileNamedFrom = ExportSettings.NativeFilenameFromType.Identifier;

			settings.OutputDataFileFormat = ExportSettings.DataFileFormat.Concordance;

			settings.ExportImages = true;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			var expectedDataFileName = $"{ExportSettings.ProductionName}_export.opt";
			var dataFiles = directory.EnumerateFiles(expectedDataFileName);
			Assert.That(dataFiles.Any());

			var nativesRootDirectory = directory.EnumerateDirectories("NATIVES", SearchOption.AllDirectories);
			var imagesRootDirectory = directory.EnumerateDirectories("IMAGES", SearchOption.AllDirectories);

			Assert.That(!imagesRootDirectory.IsNullOrEmpty());
			// we don expect any natives generted for this case
			Assert.That(nativesRootDirectory.IsNullOrEmpty());

			var actualFileCount = Directory.EnumerateFiles(imagesRootDirectory.First().FullName, "*", SearchOption.AllDirectories).Count();

			// Production should generate additioanl image for AZIPPER_0011318
			var expectedFilesCount = documentsTestData.Images.Rows.Count + 1;

			Assert.That(expectedFilesCount, Is.EqualTo(actualFileCount));
		}
	}
}
