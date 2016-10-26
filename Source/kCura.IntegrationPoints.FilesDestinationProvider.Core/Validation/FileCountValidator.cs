using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
	public class FileCountValidator : IFileCountValidator
	{
		public ExportSettingsValidationResult Validate(int totalDocCount)
		{
			var result = new ExportSettingsValidationResult {IsValid = totalDocCount > 0};
			if (!result.IsValid)
			{
				result.Message = "....";
			}
			return result;
		}
	}
}