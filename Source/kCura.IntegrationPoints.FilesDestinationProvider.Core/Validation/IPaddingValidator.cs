using kCura.IntegrationPoints.Domain.Models;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
	public interface IPaddingValidator
	{
		ValidationResult Validate(int workspaceId, ExportFile exportFile, int totalDocCount);
	}
}