using System.Collections.Generic;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Validation.Parts.Entity
{
    internal class FirstAndLastNameMappedValidator : EntityValidatorBase
    {
        private readonly ILogger<FirstAndLastNameMappedValidator> _logger;

        public FirstAndLastNameMappedValidator(ILogger<FirstAndLastNameMappedValidator> logger)
            : base(logger.ForContext<EntityValidatorBase>())
        {
            _logger = logger;
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

            bool isFieldIncluded = IsFieldIncludedInDestinationFieldMap(fieldMap, EntityFieldNames.FirstName);
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

            bool isFieldIncluded = IsFieldIncludedInDestinationFieldMap(fieldMap, EntityFieldNames.LastName);
            if (!isFieldIncluded)
            {
                _logger.LogInformation("Field {fieldName} not found in destination FieldMap", EntityFieldNames.LastName);
                result.Add(IntegrationPointProviderValidationMessages.ERROR_MISSING_LAST_NAME_FIELD_MAP);
            }
            return result;
        }
    }
}
