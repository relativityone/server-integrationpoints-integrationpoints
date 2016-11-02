using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IExportSettingsValidationService
	{
		ValidationResult Validate(int workspaceID, IntegrationModel model);
	}
}