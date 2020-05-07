using System.Collections.Generic;
using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Process.Internals;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using kCura.Utility.Extensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	public class ItShouldExportProductionSetWithNatives : ExportTestCaseBase
	{
		private readonly ExportTestConfiguration _testConfiguration;
		private readonly ExportTestContext _testContext;

		public ItShouldExportProductionSetWithNatives(ExportTestContext testContext, ExportTestConfiguration testConfiguration)
		{
			_testConfiguration = testConfiguration;
			_testContext = testContext;
		}

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ProductionId = _testContext.ProductionArtifactID;
			settings.TypeOfExport = ExportSettings.ExportType.ProductionSet;
			settings.ExportNativesToFileNamedFrom = ExportSettings.NativeFilenameFromType.Identifier;

			settings.OutputDataFileFormat = ExportSettings.DataFileFormat.Concordance;

			settings.ExportNatives = true;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			Assert.Pass("This test checks export of images and natives from production, but IAPI does not support importing natives into production.");

			string expectedDataFileName = $"{_testConfiguration.ProductionArtifactName}_export.dat";
			IEnumerable<FileInfo> dataFiles = directory.EnumerateFiles(expectedDataFileName);
			Assert.True(dataFiles.Any());

			List<DirectoryInfo> nativesRootDirectory = directory.EnumerateDirectories("NATIVES", SearchOption.AllDirectories).ToList();
			IEnumerable<DirectoryInfo> imagesRootDirectory = directory.EnumerateDirectories("IMAGES", SearchOption.AllDirectories);

			// we don expect any images generated for this case
			Assert.True(imagesRootDirectory.IsNullOrEmpty());
			
			Assert.False(nativesRootDirectory.IsNullOrEmpty());

			int actualFileCount = Directory.EnumerateFiles(nativesRootDirectory[0].FullName, "*", SearchOption.AllDirectories).Count();

			Assert.That(actualFileCount, Is.EqualTo(documentsTestData.AllDocumentsDataTable.Rows.Count));
		}
	}
}