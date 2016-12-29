using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
	public class StopJobPermissionValidator : BasePermissionValidator
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public StopJobPermissionValidator(IRepositoryFactory repositoryFactoryFactory, ISerializer serializer, IServiceContextHelper contextHelper)
			: base(serializer, contextHelper)
		{
			_repositoryFactory = repositoryFactoryFactory;
		}

		public override string Key => Constants.IntegrationPoints.Validation.STOP;

		public override ValidationResult Validate(IntegrationPointProviderValidationModel model)
		{
			var result = new ValidationResult();
			
			IPermissionRepository sourcePermissionRepository = _repositoryFactory.GetPermissionRepository(ContextHelper.WorkspaceID);

			if (!sourcePermissionRepository.UserHasArtifactInstancePermission(Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid,
				model.ArtifactId, ArtifactPermission.Edit))
			{
				result.Add(Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_INTEGRATIONPOINT);
			}
			if (!sourcePermissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.Edit))
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_NO_EDIT);
			}

			return result;
		}
	}
}