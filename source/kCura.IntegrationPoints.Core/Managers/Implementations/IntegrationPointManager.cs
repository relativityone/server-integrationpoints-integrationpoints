﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
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

		private class DestinationConfiguration
		{
			public int artifactTypeID { get; set; }
		}

		public PermissionCheckDTO UserHasPermissionToRunJob(int workspaceArtifactId, IntegrationPointDTO integrationPointDto, Constants.SourceProvider? sourceProvider = null)
		{

			IPermissionRepository sourcePermissionRepository = _repositoryFactory.GetPermissionRepository(workspaceArtifactId);
			IArtifactGuidRepository artifactGuidRepository = _repositoryFactory.GetArtifactGuidRepository(workspaceArtifactId);

			bool sourceWorkspacePermission = sourcePermissionRepository.UserHasPermissionToAccessWorkspace();
			bool integrationPointTypeViewPermission =
				sourcePermissionRepository.UserHasArtifactTypePermission(Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, ArtifactPermission.View);
			bool integrationPointInstanceViewPermission = sourcePermissionRepository.UserHasArtifactInstancePermission(Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.View);

			DestinationConfiguration destinationConfiguration = JsonConvert.DeserializeObject<DestinationConfiguration>(integrationPointDto.DestinationConfiguration);

			bool sourceImportPermission = true;
			bool destinationImportPermission = true;
			bool destinationRdoPermissions = false;
			bool destinationWorkspacePermission = true;
			bool savedSearchPermissions = true;
			bool savedSearchIsPublic = true;
			bool exportPermission = true;
			bool sourceDocumentEditPermissions = true;

			if (sourceProvider.HasValue)
			{
				sourceProvider = this.GetSourceProvider(workspaceArtifactId, integrationPointDto);
			}

			if (sourceProvider == Constants.SourceProvider.Relativity)
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
					destinationConfiguration.artifactTypeID, 
					new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Add });
				sourceDocumentEditPermissions = sourcePermissionRepository.UserCanEditDocuments();

				SavedSearchDTO savedSearch = savedSearchRepository.RetrieveSavedSearch();
				if (savedSearch == null)
				{
					savedSearchPermissions = false;
				}
				else
				{
					savedSearchIsPublic = savedSearch.Owner == 0;
				}
			}
			else
			{
				sourceImportPermission = sourcePermissionRepository.UserCanImport();
				destinationRdoPermissions = sourcePermissionRepository.UserHasArtifactTypePermissions(
					destinationConfiguration.artifactTypeID, 
					new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Add });
			}

			var errorMessages = new List<string>();
			bool userHasAllPermissions = true;

			if (!sourceWorkspacePermission)
			{
				userHasAllPermissions = false;
				errorMessages.Add("User does not have permission to access this workspace");
			}

			if (!integrationPointTypeViewPermission)
			{
				userHasAllPermissions = false;
				errorMessages.Add("User does not have permissions to view Integration Points.");
			}

			if (!integrationPointInstanceViewPermission)
			{
				userHasAllPermissions = false;
				errorMessages.Add("User does not have permission to view selected Integration Point.");
			}

			if (!sourceImportPermission)
			{
				userHasAllPermissions = false;
				errorMessages.Add("User does not have import permissions for this workspace.");
			}

			if (!destinationRdoPermissions)
			{
				userHasAllPermissions = false;
				errorMessages.Add("User must have destination RDO view, edit, and add permissions.");
			}

			if (sourceProvider == Constants.SourceProvider.Relativity)
			{
				// Relativity provider specific permissions
				if (!destinationWorkspacePermission)
				{
					userHasAllPermissions = false;
					errorMessages.Add("User does not have access to the destination workspace.");
				}

				if (!destinationImportPermission)
				{
					userHasAllPermissions = false;
					errorMessages.Add("User does not have permission to import in the destination workspace.");
				}

				if (!exportPermission)
				{
					userHasAllPermissions = false;
					errorMessages.Add("User does not have export permission in the source workspace.");
				}

				if (!sourceDocumentEditPermissions)
				{
					userHasAllPermissions = false;
					errorMessages.Add("User does not have document edit permissions in source workspace");
				}

				if (!savedSearchPermissions)
				{
					userHasAllPermissions = false;
					errorMessages.Add("User does not have access to saved search.");
				}

				if (!savedSearchIsPublic)
				{
					userHasAllPermissions = false;
					errorMessages.Add("The saved search must be public.");
				}
			}

			var permissionCheck = new PermissionCheckDTO()
			{
				Success = userHasAllPermissions,
				ErrorMessages = errorMessages.ToArray()
			};

			return permissionCheck;
		}
	}
}
