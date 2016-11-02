using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
	public class FileCountValidator : IFileCountValidator
	{
		private const string _FILE_COUNT_WARNING = "There are no items to export. Verify your source location.";

		public ValidationResult Validate(int totalDocCount)
		{
			var result = new ValidationResult {IsValid = totalDocCount > 0};
			if (!result.IsValid)
			{
				result.Message = _FILE_COUNT_WARNING;
			}
			return result;
		}
	}
}