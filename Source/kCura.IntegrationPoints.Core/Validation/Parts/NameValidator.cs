using System;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
	public class NameValidator : IValidator
	{
		public const string ERROR_INTEGRATION_POINT_EMPTY = "Integration Point name cannot be empty.";

		public string Key => Constants.IntegrationPointProfiles.Validation.NAME;

		public ValidationResult Validate(object value)
		{
			var result = new ValidationResult();

			if (String.IsNullOrWhiteSpace(value as string))
			{
				result.Add(ERROR_INTEGRATION_POINT_EMPTY);
			}

			return result;
		}
	}
}