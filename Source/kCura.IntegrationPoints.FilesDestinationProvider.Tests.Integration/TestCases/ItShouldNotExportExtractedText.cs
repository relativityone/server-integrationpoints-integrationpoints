using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	class ItShouldNotExportExtractedText :  ExportTestCaseBase
	{
		public override void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
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
