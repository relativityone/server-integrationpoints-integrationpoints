using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
	public class FileCountValidator : IFileCountValidator
	{
		private const string _FILE_COUNT_WARNING = "There are no items to export. Verify your source location.";

		public ValidationResult Validate(int totalDocCount)
		{
			return (totalDocCount > 0) ? 
				new ValidationResult() : 
				new ValidationResult(false, _FILE_COUNT_WARNING);
		}
	}
}