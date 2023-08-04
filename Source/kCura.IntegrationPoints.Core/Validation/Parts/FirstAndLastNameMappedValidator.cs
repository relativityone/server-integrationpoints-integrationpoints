using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
    public class FirstAndLastNameMappedValidator : BasePartsValidator<IntegrationPointProviderValidationModel>
    {
        private readonly IAPILog _logger;

        public override string Key => ObjectTypeGuids.Entity.ToString();

        public FirstAndLastNameMappedValidator(IAPILog logger)
        {
            _logger = logger.ForContext<FirstAndLastNameMappedValidator>();
        }

        public override ValidationResult Validate(IntegrationPointProviderValidationModel value)
        {
            var result = new ValidationResult();
            List<FieldMap> fieldsMap = value.FieldsMap;

            result.Add(ValidateFirstNameMapped(fieldsMap));
            result.Add(ValidateLastNameMapped(fieldsMap));

            return result;
        }

        private ValidationResult ValidateFirstNameMapped(List<FieldMap> fieldMap)
        {
            var result = new ValidationResult();

            bool isFieldIncluded = CheckIfFieldIsIncludedInDestinationFieldMap(fieldMap, EntityFieldNames.FirstName);
            if (!isFieldIncluded)
            {
                _logger.LogInformation("Field {fieldName} not found in destination FieldMap", EntityFieldNames.FirstName);
                result.Add(IntegrationPointProviderValidationMessages.ERROR_MISSING_FIRST_NAME_FIELD_MAP);
            }
            return result;
        }

        private ValidationResult ValidateLastNameMapped(List<FieldMap> fieldMap)
        {
            var result = new ValidationResult();

            bool isFieldIncluded = CheckIfFieldIsIncludedInDestinationFieldMap(fieldMap, EntityFieldNames.LastName);
            if (!isFieldIncluded)
            {
                _logger.LogInformation("Field {fieldName} not found in destination FieldMap", EntityFieldNames.LastName);
                result.Add(IntegrationPointProviderValidationMessages.ERROR_MISSING_LAST_NAME_FIELD_MAP);
            }
            return result;

        }

        private bool CheckIfFieldIsIncludedInDestinationFieldMap(List<FieldMap> fieldMapList, string fieldName)
        {
            _logger.LogInformation("Validating destination FieldMap for presence of field: {fieldName}", fieldName);
            return fieldMapList.Any(x => x.DestinationField.DisplayName == fieldName);
        }

    }
}
