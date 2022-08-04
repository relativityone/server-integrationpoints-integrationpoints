using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Domain.Constants;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts
{
    public class PermissionValidator : BasePermissionValidator
    {
        private readonly IAPILog _logger;
        private readonly IRepositoryFactory _repositoryFactory;

        public PermissionValidator(IRepositoryFactory repositoryFactoryFactory, ISerializer serializer, IServiceContextHelper contextHelper, IAPILog logger) : base(serializer, contextHelper)
        {
            _logger = logger.ForContext<PermissionValidator>();
            _repositoryFactory = repositoryFactoryFactory;
        }

        public override string Key => IntegrationPointPermissionValidator.GetProviderValidatorKey(
            Constants.RELATIVITY_PROVIDER_GUID,
            IntegrationPoints.Core.Constants.IntegrationPoints.LOAD_FILE_DESTINATION_PROVIDER_GUID
        );

        public override ValidationResult Validate(IntegrationPointProviderValidationModel model)
        {
            var result = new ValidationResult();
            ExportUsingSavedSearchSettings exportSettings = Serializer.Deserialize<ExportUsingSavedSearchSettings>(model.SourceConfiguration);

            ExportSettings.ExportType exportType;
            if (!Enum.TryParse(exportSettings.ExportType, out exportType))
            {
                _logger.LogError("Failed to retrieve ExportType from export settings. Export type value: {exportType}", exportSettings.ExportType);
                throw new IntegrationPointsException(IntegrationPointsExceptionMessages.ERROR_OCCURED_CONTACT_ADMINISTRATOR)
                {
                    ExceptionSource = IntegrationPointsExceptionSource.VALIDATION,
                    ShouldAddToErrorsTab = false
                };
            }

            var permissionRepository = _repositoryFactory.GetPermissionRepository(ContextHelper.WorkspaceID);

            if ((exportType == ExportSettings.ExportType.Folder)
                || (exportType == ExportSettings.ExportType.FolderAndSubfolders))
            {
                var hasFolderPermission = permissionRepository.UserHasArtifactInstancePermission(
                    (int)ArtifactType.Folder, exportSettings.FolderArtifactId, ArtifactPermission.View);

                if (!hasFolderPermission)
                {
                    result.Add(FileDestinationProviderValidationMessages.EXPORT_FOLDER_NO_VIEW);
                }
            }
            else if (exportType == ExportSettings.ExportType.ProductionSet)
            {
                var hasProductionPermission = permissionRepository.UserHasArtifactInstancePermission(
                    (int)ArtifactType.Production, exportSettings.ProductionId, ArtifactPermission.View);
                if (!hasProductionPermission)
                {
                    result.Add(FileDestinationProviderValidationMessages.EXPORT_PRODUCTION_NO_VIEW);
                }
            }

            return result;
        }
    }
}