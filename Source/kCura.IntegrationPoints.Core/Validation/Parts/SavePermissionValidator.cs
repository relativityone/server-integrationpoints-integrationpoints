using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain.Models;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
	public class SavePermissionValidator : BasePermissionValidator
	{
		private readonly IRepositoryFactory _repositoryFactory;
		public SavePermissionValidator(IRepositoryFactory repositoryFactoryFactory, ISerializer serializer) : base(serializer)
		{
			_repositoryFactory = repositoryFactoryFactory;
		}
		public override string Key => Constants.IntegrationPoints.Validation.SAVE;

		public override ValidationResult Validate(IntegrationPointProviderValidationModel model)
		{
			var result = new ValidationResult();

			var integrationPointObjectTypeGuid = new Guid(ObjectTypeGuids.IntegrationPoint);

			SourceConfiguration sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(model.SourceConfiguration);

			var permissionRepository = _repositoryFactory.GetPermissionRepository(sourceConfiguration.SourceWorkspaceArtifactId);

			if (model.ArtifactId > 0) // IP exists -- Edit permissions check
			{
				if (!permissionRepository.UserHasArtifactTypePermission(integrationPointObjectTypeGuid, ArtifactPermission.Edit))
				{
					result.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_TYPE_NO_EDIT);
				}

				if (!permissionRepository.UserHasArtifactInstancePermission(integrationPointObjectTypeGuid, model.ArtifactId, ArtifactPermission.Edit))
				{
					result.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_INSTANCE_NO_EDIT);
				}
			}
			else // IP is new -- Create permissions check
			{
				if (!permissionRepository.UserHasArtifactTypePermission(integrationPointObjectTypeGuid, ArtifactPermission.Create))
				{
					result.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_TYPE_NO_CREATE);
				}
			}

			return result;
		}
	}
}
