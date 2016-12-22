using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator
{
	public class RelativityProviderPermissionValidator : BasePermissionValidator
	{
		private readonly IRepositoryFactory _repositoryFactory;
		public RelativityProviderPermissionValidator(IRepositoryFactory repositoryFactoryFactory, ISerializer serializer) : base(serializer)
		{
			_repositoryFactory = repositoryFactoryFactory;
		}

		public override string Key => IntegrationPointPermissionValidator.GetProviderValidatorKey(IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID, Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString());

		public override ValidationResult Validate(IntegrationPointProviderValidationModel model)
		{
			var result = new ValidationResult();

			SourceConfiguration sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(model.SourceConfiguration);
			DestinationConfiguration destinationConfiguration = _serializer.Deserialize<DestinationConfiguration>(model.DestinationConfiguration);

			var sourceWorkspacePermissionRepository = _repositoryFactory.GetPermissionRepository(sourceConfiguration.SourceWorkspaceArtifactId);
			var destinationWorkspacePermissionRepository = _repositoryFactory.GetPermissionRepository(sourceConfiguration.TargetWorkspaceArtifactId);

			if (!sourceWorkspacePermissionRepository.UserCanExport())
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_WORKSPACE_NO_EXPORT);
			}

			if (!destinationWorkspacePermissionRepository.UserHasPermissionToAccessWorkspace())
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.DESTINATION_WORKSPACE_NO_ACCESS);
			}

			if (!destinationWorkspacePermissionRepository.UserCanImport())
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.DESTINATION_WORKSPACE_NO_IMPORT);
			}

			if (!destinationWorkspacePermissionRepository.UserHasArtifactTypePermissions(destinationConfiguration.ArtifactTypeId,
				new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create }))
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.MISSING_DESTINATION_RDO_PERMISSIONS);
			}

			if (!sourceWorkspacePermissionRepository.UserCanEditDocuments())
			{
				result.Add(Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_DOCUMENTS);
			}

			return result;
		}
	}
}
