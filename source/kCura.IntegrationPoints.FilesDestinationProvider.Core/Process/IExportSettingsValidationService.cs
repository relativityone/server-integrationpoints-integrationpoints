using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	public interface IExportSettingsValidationService
	{
		ExportSettingsValidationResult Validate(int workspaceID, IntegrationModel model);
	}
}