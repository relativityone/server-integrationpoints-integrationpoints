using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Implementation
{
	public class FieldMappingsValidator : IValidator
	{
		private readonly ISerializer _serializer;
		public string Key => Constants.IntegrationPoints.Validation.FIELD_MAP;

		public const string ERROR_SOURCE_FIELD_NOT_MAPPED = "All selected fields must be mapped. Source field not mapped.";
		public const string ERROR_DESTINATION_FIELD_NOT_MAPPED = "All selected fields must be mapped. Destination field not mapped.";
		public const string ERROR_SOURCE_AND_DESTINATION_FIELDS_NOT_MAPPED = "All selected fields must be mapped. Destination and Source fields not mapped.";
		public const string ERROR_FIELD_MAP_INVALID_FORMAT = "Field Map is in invalid format. ";

		public FieldMappingsValidator(ISerializer serializer)
		{
			_serializer = serializer;
		}

		public ValidationResult Validate(object value)
		{
			var result = new ValidationResult();
			var fieldMappings = value as string;

			try
			{
				var fieldMaps = _serializer.Deserialize<IEnumerable<FieldMap>>(fieldMappings);

				foreach (FieldMap fieldMap in fieldMaps)
				{
					result.Add(ValidateFieldMapped(fieldMap));
					result.Add(ValidateFieldIdentifierMappedCorrectly(fieldMap));
				}

			}
			catch (Exception ex)
			{
				result.Add(ERROR_FIELD_MAP_INVALID_FORMAT + "Error message: " + ex.Message);
				return result;
			}

			return result;
		}

		private static ValidationResult ValidateFieldMapped(FieldMap fieldMap)
		{
			var result = new ValidationResult();
			
			if (fieldMap.SourceField == null && fieldMap.DestinationField == null)
			{
				result.Add(ERROR_SOURCE_AND_DESTINATION_FIELDS_NOT_MAPPED);
			}
			else if (fieldMap.SourceField == null)
			{
				result.Add(ERROR_SOURCE_FIELD_NOT_MAPPED);
			}
			else if (fieldMap.DestinationField == null)
			{
				result.Add(ERROR_DESTINATION_FIELD_NOT_MAPPED);
			}

			return result;
		}

		private ValidationResult ValidateFieldIdentifierMappedCorrectly(FieldMap fieldMap)
		{
			var result = new ValidationResult();



			return result;
		}
	}
}