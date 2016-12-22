﻿using System;
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
	public class PermissionValidator : BasePermissionValidator
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public PermissionValidator(IRepositoryFactory repositoryFactoryFactory, ISerializer serializer) : base(serializer)
		{
			_repositoryFactory = repositoryFactoryFactory;
		}

		public override string Key => Constants.IntegrationPoints.Validation.INTEGRATION_POINT;

		public override ValidationResult Validate(IntegrationPointProviderValidationModel model)
		{
			var result = new ValidationResult();
		
			SourceConfiguration sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(model.SourceConfiguration);

			var permissionRepository = _repositoryFactory.GetPermissionRepository(sourceConfiguration.SourceWorkspaceArtifactId);

			if (!permissionRepository.UserHasPermissionToAccessWorkspace())
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.CURRENT_WORKSPACE_NO_ACCESS);
			}

			if (!permissionRepository.UserHasArtifactTypePermission(Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, ArtifactPermission.View))
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_TYPE_NO_VIEW);
			}

			if (!permissionRepository.UserHasArtifactInstancePermission(Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, model.ArtifactId, ArtifactPermission.View))
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_INSTANCE_NO_VIEW);
			}

			if (!permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.Create))
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_TYPE_NO_ADD);
			}

			var sourceProviderGuid = new Guid(ObjectTypeGuids.SourceProvider);
			if (!permissionRepository.UserHasArtifactTypePermission(sourceProviderGuid, ArtifactPermission.View))
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_PROVIDER_NO_VIEW);
			}

			var destinationProviderGuid = new Guid(ObjectTypeGuids.DestinationProvider);
			if (!permissionRepository.UserHasArtifactTypePermission(destinationProviderGuid, ArtifactPermission.View))
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.DESTINATION_PROVIDER_NO_VIEW);
			}

			if (!permissionRepository.UserHasArtifactInstancePermission(sourceProviderGuid, model.SourceProviderArtifactId, ArtifactPermission.View))
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_PROVIDER_NO_INSTANCE_VIEW);
			}
			
			return result;
		}
	}
}
