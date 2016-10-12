using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
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
			ISourceProviderRepository sourceProviderRepository = _repositoryFactory.GetSourceProviderRepository(workspaceArtifactId);
			SourceProviderDTO sourceProviderDto = sourceProviderRepository.Read(integrationPointDto.SourceProvider.Value);

			IDestinationProviderRepository destinationProviderRepository = _repositoryFactory.GetDestinationProviderRepository(workspaceArtifactId);
			DestinationProviderDTO destinationProviderDto = destinationProviderRepository.Read(integrationPointDto.DestinationProvider.Value);

			var sourceProvider = Constants.SourceProvider.Other;

			if ((sourceProviderDto.Identifier == new Guid(Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID)) &&
				(destinationProviderDto.Identifier == new Guid(Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID)))
			{
				sourceProvider = Constants.SourceProvider.Relativity;
			}

			return sourceProvider;
		}

		public PermissionCheckDTO UserHasPermissionToViewErrors(int workspaceArtifactId)
		{
			IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(workspaceArtifactId);
			var errorMessages = new List<string>();

			if (!permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.View))
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_NO_VIEW);
			}

			if (!permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistoryError), ArtifactPermission.View))
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_ERROR_NO_VIEW);
			}

			var permissionCheck = new PermissionCheckDTO
			{
				ErrorMessages = errorMessages.ToArray()
			};

			return permissionCheck;
		}

		public PermissionCheckDTO UserHasPermissionToSaveIntegrationPoint(int sourceWorkspaceArtifactId, IntegrationPointDTO integrationPointDto,
			Constants.SourceProvider? sourceProvider = null)
		{
			IPermissionRepository sourceWorkspacePermissionRepository = _repositoryFactory.GetPermissionRepository(sourceWorkspaceArtifactId);
			var errorMessages = new List<string>();

			// Get the save only permissions
			var integrationPointObjectTypeGuid = new Guid(ObjectTypeGuids.IntegrationPoint);
			if (integrationPointDto.ArtifactId > 0) // IP exists -- Edit permissions check
			{
				if (!sourceWorkspacePermissionRepository.UserHasArtifactTypePermission(integrationPointObjectTypeGuid, ArtifactPermission.Edit))
				{
					errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_TYPE_NO_EDIT);
				}

				if (!sourceWorkspacePermissionRepository.UserHasArtifactInstancePermission(integrationPointObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.Edit))
				{
					errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_INSTANCE_NO_EDIT);
				}
			}
			else // IP is new -- Create permissions check
			{
				if (!sourceWorkspacePermissionRepository.UserHasArtifactTypePermission(integrationPointObjectTypeGuid, ArtifactPermission.Create))
				{
					errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_TYPE_NO_CREATE);
				}
			}

			// Get the run permissions
			PermissionCheckDTO runPermissionCheck = UserHasPermissionToRunJob(sourceWorkspaceArtifactId, integrationPointDto, sourceProvider);
			if (runPermissionCheck.ErrorMessages != null)
			{
				errorMessages.AddRange(runPermissionCheck.ErrorMessages);
			}

			// Merge the save and run permissions
			var permissionCheck = new PermissionCheckDTO
			{
				ErrorMessages = errorMessages.ToArray()
			};

			return permissionCheck;
		}

		public virtual PermissionCheckDTO UserHasPermissionToRunJob(int workspaceArtifactId, IntegrationPointDTO integrationPointDto,
			Constants.SourceProvider? sourceProvider = null)
		{
			IPermissionRepository sourcePermissionRepository = _repositoryFactory.GetPermissionRepository(workspaceArtifactId);

			bool sourceWorkspacePermission = sourcePermissionRepository.UserHasPermissionToAccessWorkspace();
			bool integrationPointTypeViewPermission =
				sourcePermissionRepository.UserHasArtifactTypePermission(Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, ArtifactPermission.View);
			bool integrationPointInstanceViewPermission = sourcePermissionRepository.UserHasArtifactInstancePermission(
				Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.View);
			bool jobHistoryAddPermission = sourcePermissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.Create);

			var destinationProviderGuid = new Guid(ObjectTypeGuids.DestinationProvider);
			bool destinationProviderTypeView = sourcePermissionRepository.UserHasArtifactTypePermission(destinationProviderGuid,
				ArtifactPermission.View);

			var sourceProviderGuid = new Guid(ObjectTypeGuids.SourceProvider);
			bool sourceProviderTypeView = sourcePermissionRepository.UserHasArtifactTypePermission(sourceProviderGuid,
				ArtifactPermission.View);
			bool sourceProviderInstanceView = sourcePermissionRepository.UserHasArtifactInstancePermission(sourceProviderGuid, integrationPointDto.SourceProvider.Value,
				ArtifactPermission.View);

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
				sourceProvider = GetSourceProvider(workspaceArtifactId, integrationPointDto);
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
					new[] {ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create});
				sourceDocumentEditPermissions = sourcePermissionRepository.UserCanEditDocuments();

				// Important Note: If the saved search is null, that means it either doesn't exist or the current user does not have permissions to it.
				// Make sure to never give information the user is not privy to
				// (i.e. if they don't have access to the saved search, don't tell them that it is also not public
				SavedSearchDTO savedSearch = savedSearchRepository.RetrieveSavedSearch();
				if (savedSearch != null)
				{
					savedSearchPermissions = true;
					savedSearchIsPublic = string.IsNullOrEmpty(savedSearch.Owner);
				}
			}
			else
			{
				sourceImportPermission = sourcePermissionRepository.UserCanImport();
				destinationRdoPermissions = sourcePermissionRepository.UserHasArtifactTypePermissions(
					destinationConfiguration.ArtifactTypeId,
					new[] {ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create});
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

			if (!destinationProviderTypeView)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.DESTINATION_PROVIDER_NO_VIEW);
			}

			if (!sourceProviderTypeView)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_PROVIDER_NO_VIEW);
			}

			if (!sourceProviderInstanceView)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_PROVIDER_NO_INSTANCE_VIEW);
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

			var permissionCheck = new PermissionCheckDTO
			{
				ErrorMessages = errorMessages.ToArray()
			};

			return permissionCheck;
		}

		public PermissionCheckDTO UserHasPermissionToStopJob(int workspaceArtifactId, int integrationPointArtifactId)
		{
			IPermissionRepository sourcePermissionRepository = _repositoryFactory.GetPermissionRepository(workspaceArtifactId);
			bool hasPermissionToEditIntegrationPoint = sourcePermissionRepository.UserHasArtifactInstancePermission(Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid,
				integrationPointArtifactId, ArtifactPermission.Edit);
			bool hasPermissionToEditJobHistory = sourcePermissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.Edit);

			List<string> errorMessages = new List<string>();

			if (!hasPermissionToEditIntegrationPoint)
			{
				errorMessages.Add(Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_INTEGRATIONPOINT);
			}
			if (!hasPermissionToEditJobHistory)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_NO_EDIT);
			}

			PermissionCheckDTO result = new PermissionCheckDTO
			{
				ErrorMessages = errorMessages.ToArray()
			};
			return result;
		}
	}
}