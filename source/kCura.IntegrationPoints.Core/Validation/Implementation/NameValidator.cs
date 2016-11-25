using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Utility.Extensions;

namespace kCura.IntegrationPoints.Core.Validation.Implementation
{
	public class NameValidator : IValidator
	{
		public string Key => Constants.IntegrationPoints.Validation.NAME;

		public const string ERROR_INTEGRATION_POINT_EMPTY = "Integration Point name can not be empty.";

		public ValidationResult Validate(object value)
		{
			var name = value as string;

			var result = new ValidationResult();

			if(name.IsNullOrEmpty()) { result.Add(ERROR_INTEGRATION_POINT_EMPTY);}

			return result;
		}
	}
}
