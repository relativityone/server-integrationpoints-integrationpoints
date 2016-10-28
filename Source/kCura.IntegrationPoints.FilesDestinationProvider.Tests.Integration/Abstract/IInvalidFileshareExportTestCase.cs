using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract
{
	public interface IInvalidFileshareExportTestCase
	{
		ExportSettings Prepare(ExportSettings settings);

		void Verify();
	}
}