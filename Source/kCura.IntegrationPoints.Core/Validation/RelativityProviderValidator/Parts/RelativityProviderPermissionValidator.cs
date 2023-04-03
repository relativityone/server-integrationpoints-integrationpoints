using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
    public class RelativityProviderPermissionValidator : BasePermissionValidator
    {
        private readonly IRelativityProviderValidatorsFactory _validatorsFactory;

        public RelativityProviderPermissionValidator(ISerializer serializer, IServiceContextHelper contextHelper, IRelativityProviderValidatorsFactory validatorsFactory)
            : base(serializer, contextHelper)
        {
            _validatorsFactory = validatorsFactory;
        }

        public override string Key => IntegrationPointPermissionValidator.GetProviderValidatorKey(Domain.Constants.RELATIVITY_PROVIDER_GUID, Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString());

        public override ValidationResult Validate(IntegrationPointProviderValidationModel model)
        {
            var result = new ValidationResult();
            if (result.IsValid)
            {
                result.Add(ValidateSourceWorkspacePermission(model));
                result.Add(ValidateDestinationWorkspacePermission(model));
                result.Add(ValidateSourceWorkspaceProductionPermission(model));
            }
            return result;
        }

        private ValidationResult ValidateSourceWorkspacePermission(IntegrationPointProviderValidationModel model)
        {
            IRelativityProviderSourceWorkspacePermissionValidator sourceWorkspacePermissionValidator = _validatorsFactory.CreateSourceWorkspacePermissionValidator();
            return sourceWorkspacePermissionValidator.Validate(ContextHelper.WorkspaceID, model.ArtifactTypeId);
        }

        private ValidationResult ValidateDestinationWorkspacePermission(IntegrationPointProviderValidationModel model)
        {
            var result = new ValidationResult();

            SourceConfiguration sourceConfiguration = Serializer.Deserialize<SourceConfiguration>(model.SourceConfiguration);

            IRelativityProviderDestinationWorkspaceExistenceValidator destinationWorkspaceExistenceValidator = _validatorsFactory
                .CreateDestinationWorkspaceExistenceValidator(sourceConfiguration.FederatedInstanceArtifactId, model.SecuredConfiguration);

            result.Add(destinationWorkspaceExistenceValidator.Validate(sourceConfiguration));

            if (!result.IsValid)
            {
                return result; // destination workspace doesnt exist
            }

            IRelativityProviderDestinationWorkspacePermissionValidator destinationWorkspacePermissionValidator = _validatorsFactory
                .CreateDestinationWorkspacePermissionValidator(sourceConfiguration.FederatedInstanceArtifactId, model.SecuredConfiguration);

            result.Add(destinationWorkspacePermissionValidator.Validate(
                sourceConfiguration.TargetWorkspaceArtifactId,
                model.DestinationConfiguration.GetDestinationArtifactTypeId(),
                model.CreateSavedSearch));

            if (!result.IsValid)
            {
                return result; // no permission to destination workspace
            }

            result.Add(ValidateDestinationFolderPermissions(model, sourceConfiguration));

            return result;
        }

        private ValidationResult ValidateDestinationFolderPermissions(IntegrationPointProviderValidationModel model, SourceConfiguration sourceConfiguration)
        {
            ValidationResult result = new ValidationResult();
            if (model.DestinationConfiguration.DestinationFolderArtifactId > 0)
            {
                IRelativityProviderDestinationFolderPermissionValidator destinationFolderPermissionValidator = _validatorsFactory.CreateDestinationFolderPermissionValidator(
                    model.DestinationConfiguration.CaseArtifactId,
                    sourceConfiguration.FederatedInstanceArtifactId,
                    model.SecuredConfiguration);

                bool useFolderPath = model.DestinationConfiguration.UseFolderPathInformation || model.DestinationConfiguration.UseDynamicFolderPath;
                result.Add(destinationFolderPermissionValidator.Validate(
                    model.DestinationConfiguration.DestinationFolderArtifactId,
                    useFolderPath,
                    model.DestinationConfiguration.MoveExistingDocuments));
            }

            return result;
        }

        private ValidationResult ValidateSourceWorkspaceProductionPermission(IntegrationPointProviderValidationModel model)
        {
            SourceConfiguration sourceConfiguration = Serializer.Deserialize<SourceConfiguration>(model.SourceConfiguration);

            var result = new ValidationResult();
            if (sourceConfiguration.SourceProductionId > 0)
            {
                var validator = _validatorsFactory.CreateSourceProductionPermissionValidator(sourceConfiguration.SourceWorkspaceArtifactId);
                return validator.Validate(sourceConfiguration.SourceWorkspaceArtifactId, sourceConfiguration.SourceProductionId);
            }
            return result;
        }
    }
}
