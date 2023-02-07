using System;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
    public class ViewErrorsPermissionValidator : IViewErrorsPermissionValidator
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public ViewErrorsPermissionValidator(IRepositoryFactory repositoryFactoryFactory)
        {
            _repositoryFactory = repositoryFactoryFactory;
        }

        public ValidationResult Validate(int workspaceArtifactId)
        {
            var result = new ValidationResult();

            IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(workspaceArtifactId);

            if (!permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.View))
            {
                result.Add(Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_NO_VIEW);
            }

            if (!permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistoryError), ArtifactPermission.View))
            {
                result.Add(Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_ERROR_NO_VIEW);
            }

            return result;
        }
    }
}
