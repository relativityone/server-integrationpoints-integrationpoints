using System.IO;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Model;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract
{
	public interface IExportTestCase
	{
		ExportSettings Prepare(ExportSettings settings);

		void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData);
	}
}