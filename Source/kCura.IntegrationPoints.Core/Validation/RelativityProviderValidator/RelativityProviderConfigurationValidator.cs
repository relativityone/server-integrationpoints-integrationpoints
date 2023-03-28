using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Common.Toggles;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator
{
    public class RelativityProviderConfigurationValidator : IValidator
    {
        private readonly IAPILog _logger;
        private readonly ISerializer _serializer;
        private readonly IRelativityProviderValidatorsFactory _validatorsFactory;
        private readonly IRipToggleProvider _toggleProvider;

        public RelativityProviderConfigurationValidator(IAPILog logger, ISerializer serializer, IRelativityProviderValidatorsFactory validatorsFactory, IRipToggleProvider toggleProvider)
        {
            _logger = logger;
            _serializer = serializer;
            _validatorsFactory = validatorsFactory;
            _toggleProvider = toggleProvider;
        }

        public string Key => IntegrationPointProviderValidator.GetProviderValidatorKey(Domain.Constants.RELATIVITY_PROVIDER_GUID, Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString());

        public ValidationResult Validate(object value)
        {
            try
            {
                IntegrationPointProviderValidationModel integrationModel = CastToValidationModel(value);
                return Validate(integrationModel);
            }
            catch (IntegrationPointsException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in {validator}", nameof(RelativityProviderConfigurationValidator));
                throw new IntegrationPointsException(IntegrationPointsExceptionMessages.ERROR_OCCURED_CONTACT_ADMINISTRATOR, ex)
                {
                    ExceptionSource = IntegrationPointsExceptionSource.VALIDATION,
                    ShouldAddToErrorsTab = false
                };
            }
        }

        private ValidationResult Validate(IntegrationPointProviderValidationModel integrationModel)
        {
            SourceConfiguration sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(integrationModel.SourceConfiguration);
            ImportSettings destinationConfiguration = _serializer.Deserialize<ImportSettings>(integrationModel.DestinationConfiguration);

            var result = new ValidationResult();
            result.Add(ValidateSyncNonDocumentFlowToggle(destinationConfiguration));
            result.Add(ValidateSourceWorkspace(sourceConfiguration));
            result.Add(ValidateDestinationWorkspace(integrationModel, sourceConfiguration));
            return result;
        }

        private ValidationResult ValidateSyncNonDocumentFlowToggle(ImportSettings destinationConfiguration)
        {
            var result = new ValidationResult();

            if (destinationConfiguration.ArtifactTypeId != (int)ArtifactType.Document && !_toggleProvider.IsEnabled<EnableSyncNonDocumentFlowToggle>())
            {
                result.Add(ValidationMessages.SyncNonDocumentFlowToggleDisabled);
            }

            return result;
        }

        private ValidationResult ValidateSourceWorkspace(SourceConfiguration sourceConfiguration)
        {
            var result = new ValidationResult();

            RelativityProviderWorkspaceNameValidator sourceWorkspaceValidator = _validatorsFactory.CreateWorkspaceNameValidator("Source");
            result.Add(sourceWorkspaceValidator.Validate(sourceConfiguration.SourceWorkspaceArtifactId));

            if (!result.IsValid)
            {
                return result;
            }

            switch (sourceConfiguration.TypeOfExport)
            {
                case SourceConfiguration.ExportType.SavedSearch:
                    SavedSearchValidator savedSearchValidator = _validatorsFactory.CreateSavedSearchValidator(sourceConfiguration.SourceWorkspaceArtifactId);
                    result.Add(savedSearchValidator.Validate(sourceConfiguration.SavedSearchArtifactId));
                    break;
                case SourceConfiguration.ExportType.ProductionSet:
                    ProductionValidator productionValidator = _validatorsFactory.CreateProductionValidator(sourceConfiguration.SourceWorkspaceArtifactId);
                    result.Add(productionValidator.Validate(sourceConfiguration.SourceProductionId));
                    break;
                case SourceConfiguration.ExportType.View:
                    ViewValidator viewValidator = _validatorsFactory.CreateViewValidator(sourceConfiguration.SourceWorkspaceArtifactId);
                    result.Add(viewValidator.Validate(sourceConfiguration.SourceViewId));
                    break;
            }

            return result;
        }

        private ValidationResult ValidateDestinationWorkspace(IntegrationPointProviderValidationModel integrationModel, SourceConfiguration sourceConfiguration)
        {
            var result = new ValidationResult();

            RelativityProviderWorkspaceNameValidator destinationWorkspaceNameValidator = _validatorsFactory.CreateWorkspaceNameValidator("Destination",
                sourceConfiguration.FederatedInstanceArtifactId, integrationModel.SecuredConfiguration);
            result.Add(destinationWorkspaceNameValidator.Validate(sourceConfiguration.TargetWorkspaceArtifactId));

            ImportSettings destinationConfiguration = _serializer.Deserialize<ImportSettings>(integrationModel.DestinationConfiguration);

            if (!result.IsValid)
            {
                return result;
            }

            FieldsMappingValidator fieldMappingValidator = _validatorsFactory.CreateFieldsMappingValidator(sourceConfiguration.FederatedInstanceArtifactId, integrationModel.SecuredConfiguration);
            result.Add(fieldMappingValidator.Validate(integrationModel));

            if (destinationConfiguration.ArtifactTypeId == (int)ArtifactType.Document)
            {
                if (destinationConfiguration.DestinationFolderArtifactId > 0 && destinationConfiguration.ProductionArtifactId == 0)
                {
                    ArtifactValidator destinationFolderValidator =
                        _validatorsFactory.CreateArtifactValidator(destinationConfiguration.CaseArtifactId,
                            "Folder",
                            sourceConfiguration.FederatedInstanceArtifactId,
                            integrationModel.SecuredConfiguration);
                    result.Add(destinationFolderValidator.Validate(destinationConfiguration.DestinationFolderArtifactId));
                }
                else if (destinationConfiguration.DestinationFolderArtifactId == 0 && destinationConfiguration.ProductionArtifactId > 0)
                {
                    ImportProductionValidator importProductionValidator =
                        _validatorsFactory.CreateImportProductionValidator(sourceConfiguration.TargetWorkspaceArtifactId,
                        destinationConfiguration.FederatedInstanceArtifactId,
                        integrationModel.SecuredConfiguration);
                    result.Add(importProductionValidator.Validate(destinationConfiguration.ProductionArtifactId));
                }
                else if (destinationConfiguration.DestinationFolderArtifactId == 0 && destinationConfiguration.ProductionArtifactId == 0)
                {
                    result.Add(IntegrationPointProviderValidationMessages.ERROR_DESTINATON_LOCATION_EMPTY);
                }
            }

            return result;
        }

        private IntegrationPointProviderValidationModel CastToValidationModel(object value)
        {
            var result = value as IntegrationPointProviderValidationModel;
            if (result != null)
            {
                return result;
            }

            _logger.LogError("Converstion to {validationModel} failed. Actual type: {type}", nameof(IntegrationPointProviderValidationModel), value?.GetType());
            throw new IntegrationPointsException(IntegrationPointsExceptionMessages.ERROR_OCCURED_CONTACT_ADMINISTRATOR)
            {
                ExceptionSource = IntegrationPointsExceptionSource.VALIDATION,
                ShouldAddToErrorsTab = false
            };
        }
    }
}
