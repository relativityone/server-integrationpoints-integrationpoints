﻿using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Validation.Parts.Entity
{
    internal class OverlayFieldIdentifierValidator : EntityValidatorBase
    {
        public OverlayFieldIdentifierValidator(ILogger<OverlayFieldIdentifierValidator> log)
            : base(log.ForContext<EntityValidatorBase>())
        {
        }

        public override ValidationResult Validate(IntegrationPointProviderValidationModel value)
        {
            Logger.LogInformation("Validating OverlayFieldIdentifier for Entity Import... - OverlayFieldId: {overlayFieldId}", value.DestinationConfiguration.OverlayIdentifier);

            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(value.DestinationConfiguration.OverlayIdentifier))
            {
                Logger.LogWarning("Overlay field identifier was not set. Skip validation.");
                return result;
            }

            if (value.DestinationConfiguration.OverlayIdentifier == EntityFieldNames.FullName)
            {
                return result;
            }

            if (!IsFieldIncludedInDestinationFieldMap(value.FieldsMap, value.DestinationConfiguration.OverlayIdentifier))
            {
                Logger.LogInformation(IntegrationPointProviderValidationMessages.ERROR_OVERLAY_IDENTIFIER_FIELD_NOT_FOUND_IN_MAPPING);
                result.Add(IntegrationPointProviderValidationMessages.ERROR_OVERLAY_IDENTIFIER_FIELD_NOT_FOUND_IN_MAPPING);

                return result;
            }

            if (value.DestinationConfiguration.OverlayIdentifier != EntityFieldNames.FullName &&
                value.DestinationConfiguration.ImportOverwriteMode == ImportOverwriteModeEnum.OverlayOnly &&
                IsFieldIncludedInDestinationFieldMap(value.FieldsMap, EntityFieldNames.FullName))
            {
                Logger.LogInformation(IntegrationPointProviderValidationMessages.ERROR_OTHER_OVERLAY_IDENTIFIER_WITH_FULL_NAME_MAPPED);
                result.Add(IntegrationPointProviderValidationMessages.ERROR_OTHER_OVERLAY_IDENTIFIER_WITH_FULL_NAME_MAPPED);

                return result;
            }

            return result;
        }
    }
}
