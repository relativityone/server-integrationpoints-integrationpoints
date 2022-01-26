using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
	public class FirstAndLastNameMappedValidator : BasePartsValidator<IntegrationPointProviderValidationModel>
	{

		private readonly ISerializer _serializer;
		public override string Key => ObjectTypeGuids.Entity.ToString();

		public FirstAndLastNameMappedValidator(ISerializer serializer)
		{
			_serializer = serializer;
		}

		public override ValidationResult Validate(IntegrationPointProviderValidationModel value)
		{
			var result = new ValidationResult();
			List<FieldMap> fieldsMap = _serializer.Deserialize<List<FieldMap>>(value.FieldsMap);
			
			result.Add(ValidateFirstNameMapped(fieldsMap));
			result.Add(ValidateLastNameMapped(fieldsMap));
			
			return result;
		}

		private static ValidationResult ValidateFirstNameMapped(List<FieldMap> fieldMap)
		{
			var result = new ValidationResult();
			
			bool isFieldIncluded = CheckIfFieldIncludedInDestinationFieldMap(fieldMap, EntityFieldNames.FirstName);
			if (!isFieldIncluded)
			{
				result.Add(IntegrationPointProviderValidationMessages.ERROR_MISSING_FIRST_NAME_FIELD_MAP);
			}
			return result;
		}
		
		private static ValidationResult ValidateLastNameMapped(List<FieldMap> fieldMap)
		{
			var result = new ValidationResult();
			
			bool isFieldIncluded = CheckIfFieldIncludedInDestinationFieldMap(fieldMap, EntityFieldNames.LastName); 
			if (!isFieldIncluded)
			{
				result.Add(IntegrationPointProviderValidationMessages.ERROR_MISSING_LAST_NAME_FIELD_MAP);
			}
			return result;

		}

		private static bool CheckIfFieldIncludedInDestinationFieldMap(List<FieldMap> fieldMapList, string fieldName)
		{
			return fieldMapList.Any(x => x.DestinationField.DisplayName == fieldName);
		}

	}
}