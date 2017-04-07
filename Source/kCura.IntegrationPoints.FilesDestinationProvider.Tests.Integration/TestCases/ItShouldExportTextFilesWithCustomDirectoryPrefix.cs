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
	internal class ItShouldExportTextFilesWithCustomDirectoryPrefix : ExportTestCaseBase
	{
		private const string _PREFIX = "custom_text_prefix";

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportFullTextAsFile = true;
			settings.TextFileEncodingType = Encoding.UTF8;

			settings.SubdirectoryTextPrefix = _PREFIX;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			var textFilesRootDirectory = directory.EnumerateDirectories("TEXT", SearchOption.AllDirectories).ToList();

			var expectedDirectories = textFilesRootDirectory.SelectMany(x => x.EnumerateDirectories().Where(y => y.Name.StartsWith(_PREFIX)));

			Assert.That(expectedDirectories.Any(), $"There should be at least one folder with specified prefix {_PREFIX}");
		}
	}
}