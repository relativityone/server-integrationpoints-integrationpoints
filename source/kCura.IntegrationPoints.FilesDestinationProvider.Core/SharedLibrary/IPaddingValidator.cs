using kCura.IntegrationPoints.Domain.Models;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public interface IPaddingValidator
	{
		ExportSettingsValidationResult Validate(int workspaceId, ExportFile exportFile, int totalDocCount);
	}
}