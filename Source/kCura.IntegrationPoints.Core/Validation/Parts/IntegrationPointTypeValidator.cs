using System;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
    public class IntegrationPointTypeValidator : IValidator
    {
        private readonly IRelativityObjectManager _objectManager;
        private readonly IAPILog _logger;
        public string Key => Constants.IntegrationPointProfiles.Validation.INTEGRATION_POINT_TYPE;

        public IntegrationPointTypeValidator(IRelativityObjectManager objectManager, IAPILog logger)
        {
            _logger = logger;
            _objectManager = objectManager;
        }

        public ValidationResult Validate(object value)
        {
            IntegrationPointProviderValidationModel integrationModel = CastToValidationModel(value);
            var result = new ValidationResult();

            IntegrationPointType integrationPointType = _objectManager.Read<IntegrationPointType>(integrationModel.Type);

            if (integrationPointType == null)
            {
                result.Add(IntegrationPointProviderValidationMessages.ERROR_INTEGRATION_POINT_TYPE_INVALID);
                return result;
            }

            if (String.Equals(integrationModel.SourceProviderIdentifier, Domain.Constants.RELATIVITY_PROVIDER_GUID, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!String.Equals(integrationPointType.Identifier, Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    result.Add(IntegrationPointProviderValidationMessages.ERROR_INTEGRATION_POINT_TYPE_INVALID);
                }
            }
            else
            {
                if (!String.Equals(integrationPointType.Identifier, Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    result.Add(IntegrationPointProviderValidationMessages.ERROR_INTEGRATION_POINT_TYPE_INVALID);
                }
            }

            return result;
        }

        private IntegrationPointProviderValidationModel CastToValidationModel(object value)
        {
            var validationModel = value as IntegrationPointProviderValidationModel;
            if (validationModel != null)
            {
                return validationModel;
            }

            _logger.LogError("An error occure casting to validation model in {validator}. Actual type: {actualType}", nameof(IntegrationPointTypeValidator), value?.GetType());
            throw new IntegrationPointsException(IntegrationPointsExceptionMessages.ERROR_OCCURED_CONTACT_ADMINISTRATOR)
            {
                ExceptionSource = IntegrationPointsExceptionSource.VALIDATION,
                ShouldAddToErrorsTab = false
            };
        }
    }
}
