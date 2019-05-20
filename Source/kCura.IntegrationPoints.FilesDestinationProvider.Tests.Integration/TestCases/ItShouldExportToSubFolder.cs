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
	public class ItShouldExportToSubFolder : ExportTestCaseBase
	{
		private readonly ExportTestConfiguration _testConfiguration;

		public ItShouldExportToSubFolder(ExportTestConfiguration testConfiguration)
		{
			_testConfiguration = testConfiguration;
		}

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.IsAutomaticFolderCreationEnabled = true;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			IEnumerable<string> directories = Directory.EnumerateDirectories(directory.FullName).ToList();

			Assert.That(directories.Any());

			string outputDirectory = directories.First();
			string expectedJobStartTimeInDirectory = _testConfiguration
				.JobStart
				.ToString("s")
				.Replace(":", string.Empty);
			Assert.That(outputDirectory.Contains($"{_testConfiguration.JobName}_{expectedJobStartTimeInDirectory}"));
			Assert.That(Directory.EnumerateFiles(outputDirectory).Any());
		}
	}
}
