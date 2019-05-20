using System.Collections.Generic;
using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Process.Internals;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportOnlyProductionSetLoadFile : ExportTestCaseBase
	{
		private readonly ExportTestConfiguration _testConfiguration;
		private readonly ExportTestContext _testContext;

		public ItShouldExportOnlyProductionSetLoadFile(ExportTestContext testContext, ExportTestConfiguration testConfiguration)
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

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			string expectedDataFileName = $"{_testConfiguration.ProductionArtifactName}_export.dat";
			IEnumerable<FileInfo> dataFiles = directory.EnumerateFiles(expectedDataFileName);
			Assert.That(dataFiles.Any());

			List<DirectoryInfo> nativesRootDirectory = directory.EnumerateDirectories("NATIVES", SearchOption.AllDirectories).ToList();
			List<DirectoryInfo> imagesRootDirectory = directory.EnumerateDirectories("IMAGES", SearchOption.AllDirectories).ToList();

			Assert.That(nativesRootDirectory.Count, Is.EqualTo(0));
			Assert.That(imagesRootDirectory.Count, Is.EqualTo(0));
		}
	}
}