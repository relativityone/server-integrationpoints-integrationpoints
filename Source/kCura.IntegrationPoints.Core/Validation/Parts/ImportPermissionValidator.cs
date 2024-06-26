﻿using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
    public class ImportPermissionValidator : BasePermissionValidator
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public ImportPermissionValidator(IRepositoryFactory repositoryFactoryFactory, ISerializer serializer, IServiceContextHelper contextHelper)
            : base(serializer, contextHelper)
        {
            _repositoryFactory = repositoryFactoryFactory;
        }

        public override string Key => Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid.ToString();

        public override ValidationResult Validate(IntegrationPointProviderValidationModel model)
        {
            var result = new ValidationResult();
            var destinationConfiguration = Serializer.Deserialize<ImportSettings>(model.DestinationConfiguration);

            var permissionRepository = _repositoryFactory.GetPermissionRepository(ContextHelper.WorkspaceID);

            if (!permissionRepository.UserCanImport())
            {
                result.Add(Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT_CURRENTWORKSPACE);
            }

            if (!permissionRepository.UserHasArtifactTypePermissions(destinationConfiguration.ArtifactTypeId,
                new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create }))
            {
                result.Add(Constants.IntegrationPoints.PermissionErrors.MISSING_DESTINATION_RDO_PERMISSIONS);
            }

            return result;
        }
    }
}
