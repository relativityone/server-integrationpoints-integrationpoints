using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Validation.Parts.Entity
{
    internal abstract class EntityValidatorBase : BasePartsValidator<IntegrationPointProviderValidationModel>
    {
        protected ILogger<EntityValidatorBase> Logger;

        public override string Key => ObjectTypeGuids.Entity.ToString();

        protected EntityValidatorBase(ILogger<EntityValidatorBase> logger)
        {
            Logger = logger;
        }

        public override ValidationResult Validate(IntegrationPointProviderValidationModel value)
        {
            return new ValidationResult();
        }

        protected bool IsFieldIncludedInDestinationFieldMap(List<FieldMap> fieldMapList, string fieldName)
        {
            Logger.LogInformation("Validating destination FieldMap for presence of field: {fieldName}", fieldName);
            return fieldMapList.Any(x => x.DestinationField.DisplayName == fieldName);
        }
    }
}
