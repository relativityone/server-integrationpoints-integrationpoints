using System.IO;
using System.Linq;
using System.Text;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldNotExportExtractedText : ExportTestCaseBase
	{
		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			var textFilesRootDirectory = directory.EnumerateDirectories("TEXT", SearchOption.AllDirectories);
			Assert.That(!textFilesRootDirectory.Any());
		}

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportFullTextAsFile = false;
			settings.TextFileEncodingType = Encoding.UTF8;

			return base.Prepare(settings);
		}
	}
}