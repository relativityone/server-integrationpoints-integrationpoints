using System.Data;
using System.IO;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract
{
	public interface IExportTestCase
	{
		ExportSettings Prepare(ExportSettings settings);

		void Verify(DirectoryInfo directory, DataTable documents, DataTable images);
	}
}
