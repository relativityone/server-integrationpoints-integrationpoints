using System.IO;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract
{
	public interface IExportTestCase
	{
		ExportSettings Prepare(ExportSettings settings);

		void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData);
	}
}