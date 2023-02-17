using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
    public class ExportPermissionValidator : BasePermissionValidator
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public ExportPermissionValidator(IRepositoryFactory repositoryFactoryFactory, ISerializer serializer, IServiceContextHelper contextHelper)
            : base(serializer, contextHelper)
        {
            _repositoryFactory = repositoryFactoryFactory;
        }

        public override string Key => Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString();

        public override ValidationResult Validate(IntegrationPointProviderValidationModel model)
        {
            ValidationResult result = new ValidationResult();

            IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(ContextHelper.WorkspaceID);

            if (!permissionRepository.UserCanExport())
            {
                result.Add(Constants.IntegrationPoints.NO_PERMISSION_TO_EXPORT_CURRENTWORKSPACE);
            }

            return result;
        }
    }
}
