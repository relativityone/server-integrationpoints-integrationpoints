using System.Collections.Generic;
using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Common.Extensions.DotNet;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Process.Internals;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	class ItShouldExportProductionSetWithImages : ExportTestCaseBase
	{
		private readonly ExportTestConfiguration _testConfiguration;
		private readonly ExportTestContext _testContext;

		public ItShouldExportProductionSetWithImages(ExportTestContext testContext, ExportTestConfiguration testConfiguration)
		{
			_testConfiguration = testConfiguration;
			_testContext = testContext;
		}

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ProductionId = _testContext.ProductionArtifactID;
			settings.TypeOfExport = ExportSettings.ExportType.ProductionSet;
			settings.ImageType = ExportSettings.ImageFileType.SinglePage;
			settings.ExportNativesToFileNamedFrom = ExportSettings.NativeFilenameFromType.Identifier;

			settings.OutputDataFileFormat = ExportSettings.DataFileFormat.Concordance;

			settings.ExportImages = true;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			string expectedDataFileName = $"{_testConfiguration.ProductionArtifactName}_export.opt";
			IEnumerable<FileInfo> dataFiles = directory.EnumerateFiles(expectedDataFileName);
			Assert.That(dataFiles.Any());

			IEnumerable<DirectoryInfo> nativesRootDirectory = directory.EnumerateDirectories("NATIVES", SearchOption.AllDirectories);
			IEnumerable<DirectoryInfo> imagesRootDirectory = directory.EnumerateDirectories("IMAGES", SearchOption.AllDirectories).ToList();

			Assert.That(!imagesRootDirectory.IsNullOrEmpty());
			// we don expect any natives generated for this case
			Assert.That(nativesRootDirectory.IsNullOrEmpty());

			int actualFileCount = Directory.EnumerateFiles(imagesRootDirectory.First().FullName, "*", SearchOption.AllDirectories).Count();
			int expectedFilesCount = documentsTestData.Images.Rows.Count;

			Assert.That(expectedFilesCount, Is.EqualTo(actualFileCount));
		}
	}
}
