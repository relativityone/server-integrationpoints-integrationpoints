using System.IO;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportDataFileWithNestedMultipleChoice : MetadataExportTestCaseBase
	{
		public override string MetadataFormat => "dat";

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.OutputDataFileFormat = ExportSettings.DataFileFormat.Concordance;
			settings.ExportMultipleChoiceFieldsAsNested = true;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			var datFileInfo = GetFileInfo(directory);
			Assert.That(DataFileFormatHelper.LineNumberContains(2, "Level1\\Level2", datFileInfo));
		}
	}
}