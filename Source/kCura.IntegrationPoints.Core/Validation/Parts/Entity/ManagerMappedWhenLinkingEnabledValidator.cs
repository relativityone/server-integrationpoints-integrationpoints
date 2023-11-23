﻿using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Parts.Entity
{
    internal class ManagerMappedWhenLinkingEnabledValidator : EntityValidatorBase
    {
        public ManagerMappedWhenLinkingEnabledValidator(ILogger<ManagerMappedWhenLinkingEnabledValidator> logger)
            : base(logger.ForContext<EntityValidatorBase>())
        {
        }

        public override ValidationResult Validate(IntegrationPointProviderValidationModel value)
        {
            var result = new ValidationResult();

            Logger.LogInformation("Validating Manager Linking for Entity Import... - ManagerLink: {managerLink}", value.DestinationConfiguration.EntityManagerFieldContainsLink);

            if (value.DestinationConfiguration.EntityManagerFieldContainsLink)
            {
                if (!IsFieldIncludedInDestinationFieldMap(value.FieldsMap, EntityFieldNames.Manager))
                {
                    Logger.LogInformation(IntegrationPointProviderValidationMessages.ERROR_MISSING_MANAGER_FIELD_MAP_WHEN_MANAGER_LINKING_CONFIGURED);
                    result.Add(IntegrationPointProviderValidationMessages.ERROR_MISSING_MANAGER_FIELD_MAP_WHEN_MANAGER_LINKING_CONFIGURED);
                }
            }

            return result;
        }
    }
}
