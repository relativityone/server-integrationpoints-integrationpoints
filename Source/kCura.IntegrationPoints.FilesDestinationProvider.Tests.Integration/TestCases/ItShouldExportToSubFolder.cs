using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Model;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	public class ItShouldExportToSubFolder : ExportTestCaseBase
	{
		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.IsAutomaticFolderCreationEnabled = true;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			IEnumerable<string> dirs = Directory.EnumerateDirectories(directory.FullName);

			Assert.That(dirs.Any());

			string outputDir = dirs.First();
			Assert.That(outputDir.Contains($"{ConfigSettings.JobName}_{ConfigSettings.JobStart.ToString("s").Replace(":","")}"));
			Assert.That(Directory.EnumerateFiles(outputDir).Any());
		}
	}
}
