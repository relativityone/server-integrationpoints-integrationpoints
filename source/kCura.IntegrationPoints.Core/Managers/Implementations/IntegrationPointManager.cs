using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class IntegrationPointManager : IIntegrationPointManager
	{
		private readonly IRepositoryFactory _repositoryFactory;

		internal IntegrationPointManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public IntegrationPointDTO Read(int workspaceArtifactId, int integrationPointArtifactId)
		{
			IIntegrationPointRepository repository = _repositoryFactory.GetIntegrationPointRepository(workspaceArtifactId);

			return repository.Read(integrationPointArtifactId);
		}

		public Constants.SourceProvider GetSourceProvider(int workspaceArtifactId, IntegrationPointDTO integrationPointDto)
		{
			ISourceProviderRepository repository = _repositoryFactory.GetSourceProviderRepository(workspaceArtifactId);
			SourceProviderDTO dto = repository.Read(integrationPointDto.SourceProvider.Value);

			Constants.SourceProvider sourceProvider;
			if (dto.Identifier == new Guid(Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID))
			{
				sourceProvider = Constants.SourceProvider.Relativity;
			}
			else
			{
				sourceProvider = Constants.SourceProvider.Other;
			}

			return sourceProvider;
		}

		public PermissionCheckDTO UserHasPermissionToRunJob(int workspaceArtifactId, IntegrationPointDTO integrationPointDto, Constants.SourceProvider? sourceProvider = null)
		{

			IPermissionRepository sourcePermissionRepository = _repositoryFactory.GetPermissionRepository(workspaceArtifactId);

			bool sourceWorkspacePermission = sourcePermissionRepository.UserHasPermissionToAccessWorkspace();
			bool integrationPointTypeViewPermission =
				sourcePermissionRepository.UserHasArtifactTypePermission(Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, ArtifactPermission.View);
			bool integrationPointInstanceViewPermission = sourcePermissionRepository.UserHasArtifactInstancePermission(Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.View);
			bool jobHistoryAddPermission = sourcePermissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.Add);

			DestinationConfiguration destinationConfiguration = JsonConvert.DeserializeObject<DestinationConfiguration>(integrationPointDto.DestinationConfiguration);

			bool sourceImportPermission = false;
			bool destinationImportPermission = false;
			bool destinationRdoPermissions = false;
			bool destinationWorkspacePermission = false;
			bool savedSearchPermissions = false;
			bool savedSearchIsPublic = false;
			bool exportPermission = false;
			bool sourceDocumentEditPermissions = false;

			if (!sourceProvider.HasValue)
			{
				sourceProvider = this.GetSourceProvider(workspaceArtifactId, integrationPointDto);
			}

			bool isRelativitySourceProvider = sourceProvider == Constants.SourceProvider.Relativity;

			if (isRelativitySourceProvider)
			{
				SourceConfiguration sourceConfiguration = JsonConvert.DeserializeObject<SourceConfiguration>(integrationPointDto.SourceConfiguration);
				int destinationWorkspaceArtifactId = sourceConfiguration.TargetWorkspaceArtifactId;
				IPermissionRepository destinationPermissionRepository =
					_repositoryFactory.GetPermissionRepository(destinationWorkspaceArtifactId);
				ISavedSearchRepository savedSearchRepository = _repositoryFactory.GetSavedSearchRepository(workspaceArtifactId, sourceConfiguration.SavedSearchArtifactId);

				exportPermission = sourcePermissionRepository.UserCanExport();
				destinationWorkspacePermission = destinationPermissionRepository.UserHasPermissionToAccessWorkspace();
				destinationImportPermission = destinationPermissionRepository.UserCanImport();
				destinationRdoPermissions = destinationPermissionRepository.UserHasArtifactTypePermissions(
					destinationConfiguration.ArtifactTypeId, 
					new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Add });
				sourceDocumentEditPermissions = sourcePermissionRepository.UserCanEditDocuments();


				// Important Note: If the saved search is null, that means it either doesn't exist or the current user does not have permissions to it.
				// Make sure to never give information the user is not privy to 
				// (i.e. if they don't have access to the saved search, don't tell them that it is also not public
				SavedSearchDTO savedSearch = savedSearchRepository.RetrieveSavedSearch();
				if (savedSearch != null)
				{
					savedSearchPermissions = true;
					savedSearchIsPublic = savedSearch.Owner == 0;
				}
			}
			else
			{
				sourceImportPermission = sourcePermissionRepository.UserCanImport();
				destinationRdoPermissions = sourcePermissionRepository.UserHasArtifactTypePermissions(
					destinationConfiguration.ArtifactTypeId, 
					new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Add });
			}

			var errorMessages = new List<string>();

			if (!sourceWorkspacePermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.CURRENT_WORKSPACE_NO_ACCESS);
			}

			if (!integrationPointTypeViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_TYPE_NO_VIEW);
			}

			if (!integrationPointInstanceViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_INSTANCE_NO_VIEW);
			}

			if (!jobHistoryAddPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_TYPE_NO_ADD);
			}

			if (!isRelativitySourceProvider && !sourceImportPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT_CURRENTWORKSPACE);
			}

			if (!destinationRdoPermissions)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.MISSING_DESTINATION_RDO_PERMISSIONS);
			}

			if (isRelativitySourceProvider)
			{
				// Relativity provider specific permissions
				if (!destinationWorkspacePermission)
				{
					errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.DESTINATION_WORKSPACE_NO_ACCESS);
				}

				if (!destinationImportPermission)
				{
					errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.DESTINATION_WORKSPACE_NO_IMPORT);
				}

				if (!exportPermission)
				{
					errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_WORKSPACE_NO_EXPORT);
				}

				if (!sourceDocumentEditPermissions)
				{
					errorMessages.Add(Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_DOCUMENTS);
				}

				if (!savedSearchPermissions)
				{
					errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.SAVED_SEARCH_NO_ACCESS);
				}
				else if (!savedSearchIsPublic)
				{
					errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.SAVED_SEARCH_NOT_PUBLIC);
				}
			}

			var permissionCheck = new PermissionCheckDTO()
			{
				Success = !errorMessages.Any(),
				ErrorMessages = errorMessages.ToArray()
			};

			return permissionCheck;
		}
	}
}
