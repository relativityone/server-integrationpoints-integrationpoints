using System;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Implementation
{
	public class FieldMappingsValidator : IValidator
	{
		public string Key => Constants.IntegrationPoints.Validation.FIELD_MAP;

		public ValidationResult Validate(object value)
		{
			return new ValidationResult { IsValid = true };
		}
	}
}