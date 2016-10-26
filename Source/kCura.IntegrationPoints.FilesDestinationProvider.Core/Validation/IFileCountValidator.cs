using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
	public interface IFileCountValidator
	{
		ExportSettingsValidationResult Validate(int totalDocCount);
	}
}