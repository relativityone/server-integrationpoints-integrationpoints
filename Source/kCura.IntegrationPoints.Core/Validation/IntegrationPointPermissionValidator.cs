using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Validation
{
    public class IntegrationPointPermissionValidator : BaseIntegrationPointValidator<IPermissionValidator>, IIntegrationPointPermissionValidator
    {
        public IntegrationPointPermissionValidator(IEnumerable<IPermissionValidator> validators, IIntegrationPointSerializer serializer)
            : base(validators, serializer)
        {
        }

        public override ValidationResult Validate(
            IntegrationPointModelBase model,
            SourceProvider sourceProvider,
            DestinationProvider destinationProvider,
            IntegrationPointType integrationPointType,
            Guid objectTypeGuid,
            int userId)
        {
            IntegrationPointProviderValidationModel validationModel = CreateValidationModel(model, sourceProvider, destinationProvider, integrationPointType, objectTypeGuid, userId);
            return Validate(validationModel, sourceProvider, destinationProvider, integrationPointType);
        }

        public ValidationResult ValidateSave(
            IntegrationPointModelBase model,
            SourceProvider sourceProvider,
            DestinationProvider destinationProvider,
            IntegrationPointType integrationPointType,
            Guid objectTypeGuid,
            int userId)
        {
            var result = new ValidationResult();

            IntegrationPointProviderValidationModel validationModel = CreateValidationModel(model, sourceProvider, destinationProvider, integrationPointType, objectTypeGuid, userId);

            IEnumerable<IPermissionValidator> validators = _validatorsMap[Constants.IntegrationPoints.Validation.SAVE];
            foreach (IPermissionValidator validator in validators)
            {
                result.Add(validator.Validate(validationModel));
            }

            result.Add(Validate(validationModel, sourceProvider, destinationProvider, integrationPointType));

            return result;
        }

        public ValidationResult ValidateViewErrors(int workspaceArtifactId)
        {
            var result = new ValidationResult();

            IEnumerable<IPermissionValidator> validators = _validatorsMap[Constants.IntegrationPoints.Validation.VIEW_ERRORS];
            foreach (IPermissionValidator validator in validators)
            {
                result.Add(validator.Validate(workspaceArtifactId));
            }

            return result;
        }

        public ValidationResult ValidateStop(
            IntegrationPointModelBase model,
            SourceProvider sourceProvider,
            DestinationProvider destinationProvider,
            IntegrationPointType integrationPointType,
            Guid objectTypeGuid,
            int userId)
        {
            var result = new ValidationResult();

            IntegrationPointProviderValidationModel validationModel = CreateValidationModel(model, sourceProvider, destinationProvider, integrationPointType, objectTypeGuid, userId);

            IEnumerable<IPermissionValidator> validators = _validatorsMap[Constants.IntegrationPoints.Validation.STOP];
            foreach (IPermissionValidator validator in validators)
            {
                result.Add(validator.Validate(validationModel));
            }

            return result;
        }

        private ValidationResult Validate(
            IntegrationPointProviderValidationModel validationModel,
            SourceProvider sourceProvider,
            DestinationProvider destinationProvider,
            IntegrationPointType integrationPointType)
        {
            var result = new ValidationResult();

            IEnumerable<IPermissionValidator> permissionValidators = _validatorsMap[Constants.IntegrationPoints.Validation.INTEGRATION_POINT];
            foreach (IPermissionValidator validator in permissionValidators)
            {
                result.Add(validator.Validate(validationModel));
            }

            //workaround for import providers
            if (integrationPointType.Identifier.Equals(Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid.ToString()))
            {
                IEnumerable<IPermissionValidator> importProviderPermissionValidators = _validatorsMap[Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid.ToString()];
                foreach (IPermissionValidator validator in importProviderPermissionValidators)
                {
                    result.Add(validator.Validate(validationModel));
                }
            }

            if (integrationPointType.Identifier.Equals(Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString()))
            {
                IEnumerable<IPermissionValidator> exportProviderPermissionValidators = _validatorsMap[Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString()];
                foreach (IPermissionValidator validator in exportProviderPermissionValidators)
                {
                    result.Add(validator.Validate(validationModel));
                }
            }

            // provider-specific validation
            IEnumerable<IPermissionValidator> sourceProviderPermissionValidators = _validatorsMap[GetProviderValidatorKey(sourceProvider.Identifier, destinationProvider.Identifier)];
            foreach (IPermissionValidator validator in sourceProviderPermissionValidators)
            {
                result.Add(validator.Validate(validationModel));
            }

            IEnumerable<IPermissionValidator> nativeCopyLinksValidators = _validatorsMap[Constants.IntegrationPoints.Validation.NATIVE_COPY_LINKS_MODE];
            foreach (IPermissionValidator validator in nativeCopyLinksValidators)
            {
                result.Add(validator.Validate(validationModel));
            }

            return result;
        }

    }
}
