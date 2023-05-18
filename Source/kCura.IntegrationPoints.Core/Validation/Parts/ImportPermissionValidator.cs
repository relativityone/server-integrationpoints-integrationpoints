using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.DestinationTypes;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;

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

            var permissionRepository = _repositoryFactory.GetPermissionRepository(ContextHelper.WorkspaceID);

            if (!permissionRepository.UserCanImport())
            {
                result.Add(Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT_CURRENTWORKSPACE);
            }

            if (!permissionRepository.UserHasArtifactTypePermissions(model.DestinationConfiguration.ArtifactTypeId,
                new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create }))
            {
                IObjectTypeRepository objectTypeRepository = _repositoryFactory.GetObjectTypeRepository(ContextHelper.WorkspaceID);

                ObjectTypeDTO objectType = objectTypeRepository.GetObjectType(model.DestinationConfiguration.ArtifactTypeId);

                result.Add(Constants.IntegrationPoints.PermissionErrors.MissingDestinationRdoPermission(objectType.Name));
            }

            return result;
        }
    }
}
