using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Permission;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors.PermissionCheck
{
	internal sealed class DestinationPermissionCheck : PermissionCheckBase
	{
		private const string _SOURCE_WORKSPACE_OBJECT_TYPE_NAME = "Relativity Source Case";
		private const string _SOURCE_JOB_OBJECT_TYPE_NAME = "Relativity Source Job";

		private const string _MISSING_DESTINATION_RDO_PERMISSIONS =
			"User does not have permissions to view, edit, and add Documents in the destination workspace.";
		private const string _MISSING_DESTINATION_SAVED_SEARCH_ADD_PERMISSION =
			"User does not have permission to create saved searches in the destination workspace.";

		private readonly ISyncObjectTypeManager _syncObjectTypeManager;
		private readonly ISyncLog _logger;

		public DestinationPermissionCheck(IDestinationServiceFactoryForUser destinationServiceFactory, ISyncObjectTypeManager syncObjectTypeManager, ISyncLog logger)
			: base(destinationServiceFactory)
		{
			_syncObjectTypeManager = syncObjectTypeManager;
			_logger = logger;
		}

		public override async Task<ValidationResult> ValidateAsync(IPermissionsCheckConfiguration configuration)
		{
			var validationResult = new ValidationResult();

			validationResult.Add(await ValidateUserHasPermissionToAccessWorkspaceAsync(configuration).ConfigureAwait(false));

			validationResult.Add(await ValidateUserCanImportHasPermissionAsync(configuration).ConfigureAwait(false));

			validationResult.Add(await ValidateUserCanCreateTagsInDestinationWorkspaceAsync(configuration).ConfigureAwait(false));

			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, (int)ArtifactType.Document,
				new[] { PermissionType.View, PermissionType.Add, PermissionType.Edit }, _MISSING_DESTINATION_RDO_PERMISSIONS).ConfigureAwait(false));

			if (configuration.CreateSavedSearchForTags)
			{
				validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, (int)ArtifactType.Search,
					new[] { PermissionType.Add }, _MISSING_DESTINATION_SAVED_SEARCH_ADD_PERMISSION).ConfigureAwait(false));
			}

			validationResult.Add(await ValidateFolderPermissionsUserHasArtifactInstancePermissionAsync(configuration, (int)ArtifactType.Document,
				PermissionType.Add).ConfigureAwait(false));

			validationResult.Add(await ValidateFolderPermissionsUserHasArtifactInstancePermissionAsync(configuration, (int)ArtifactType.Folder,
				PermissionType.Add).ConfigureAwait(false));

			return validationResult;
		}

		private async Task<ValidationResult> ValidateUserHasPermissionToAccessWorkspaceAsync(IPermissionsCheckConfiguration configuration)
		{
			var artifactTypeIdentifier = new ArtifactTypeIdentifier((int)ArtifactType.Case);
			List<PermissionRef> permissionRefs = GetPermissionRefs(artifactTypeIdentifier, PermissionType.View);

			bool userHasViewPermissions = false;
			try
			{
				IList<PermissionValue> permissions = await GetPermissionsForArtifactIdAsync(-1, configuration.DestinationWorkspaceArtifactId, permissionRefs).ConfigureAwait(false);
				userHasViewPermissions = DoesUserHavePermissions(permissions);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex, "{PermissionCheck}: user does not have permission to access workspace {ArtifactId}.", nameof(DestinationPermissionCheck), configuration.DestinationWorkspaceArtifactId);
			}
			const string errorCode = "20.001";
			const string errorMessage = "User does not have sufficient permissions to access destination workspace. Contact your system administrator.";

			return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage, errorCode);
		}

		private async Task<ValidationResult> ValidateUserCanImportHasPermissionAsync(IPermissionsCheckConfiguration configuration)
		{
			const int allowImportPermissionArtifactID = 158;
			List<PermissionRef> permissionRefs = GetPermissionRefs(allowImportPermissionArtifactID);
			bool userHasViewPermissions = false;
			try
			{
				IList<PermissionValue> permissions = await GetPermissionsAsync(configuration.DestinationWorkspaceArtifactId, permissionRefs).ConfigureAwait(false);
				userHasViewPermissions = DoesUserHavePermissions(permissions, allowImportPermissionArtifactID);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex, "{PermissionCheck}: user does not have allow import permission in destination workspace {ArtifactId}.",
					nameof(DestinationPermissionCheck), configuration.DestinationWorkspaceArtifactId);
			}
			const string errorMessage = "User does not have permission to import in the destination workspace.";
			return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage);
		}

		private async Task<ValidationResult> ValidateUserCanCreateTagsInDestinationWorkspaceAsync(IPermissionsCheckConfiguration configuration)
		{
			ValidationResult validationResult = new ValidationResult();
			validationResult.Add(await ValidateUserCanCreateTagInDestinationWorkspaceAsync(configuration, _SOURCE_WORKSPACE_OBJECT_TYPE_NAME).ConfigureAwait(false));
			validationResult.Add(await ValidateUserCanCreateTagInDestinationWorkspaceAsync(configuration, _SOURCE_JOB_OBJECT_TYPE_NAME).ConfigureAwait(false));
			return validationResult;
		}

		private async Task<ValidationResult> ValidateUserCanCreateTagInDestinationWorkspaceAsync(IPermissionsCheckConfiguration configuration, string objectTypeName)
		{
			QueryResult objectTypeQueryResult = await _syncObjectTypeManager
				.QueryObjectTypeByNameAsync(configuration.DestinationWorkspaceArtifactId, objectTypeName).ConfigureAwait(false);

			if (objectTypeQueryResult.Objects.Any())
			{
				string insufficientPermissionsMessage = $"User does not have permissions to create tag: {objectTypeName}";

				int objectArtifactTypeID = await _syncObjectTypeManager.GetArtifactTypeID(configuration.DestinationWorkspaceArtifactId, objectTypeQueryResult.Objects.First().ArtifactID);

				return await ValidateUserHasArtifactTypePermissionAsync(configuration,
					objectArtifactTypeID, new[] { PermissionType.View, PermissionType.Add },
					insufficientPermissionsMessage).ConfigureAwait(false);
					
			}
			else
			{
				throw new Validation.ValidationException($"Cannot find Object Type: {objectTypeName} in Destination Workspace Artifact ID: {configuration.DestinationWorkspaceArtifactId}");
			}
		}

		private async Task<ValidationResult> ValidateUserHasArtifactTypePermissionAsync(IPermissionsCheckConfiguration configuration,
			int artifactTypeID, IEnumerable<PermissionType> artifactPermissions, string errorMessage)
		{
			var typeIdentifier = new ArtifactTypeIdentifier(artifactTypeID);
			List<PermissionRef> permissionRefs = GetPermissionRefs(typeIdentifier, artifactPermissions);

			bool userHasViewPermissions = false;
			try
			{
				IList<PermissionValue> permissions = await GetPermissionsAsync(configuration.DestinationWorkspaceArtifactId, permissionRefs).ConfigureAwait(false);
				userHasViewPermissions = DoesUserHavePermissions(permissions);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex, "{PermissionCheck}: user does not have artifact type {ArtifactTypeIdentifier} permission(s) in destination workspace {ArtifactId}.",
					nameof(DestinationPermissionCheck), artifactTypeID, configuration.DestinationWorkspaceArtifactId);
			}
			return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage);
		}

		private async Task<ValidationResult> ValidateFolderPermissionsUserHasArtifactInstancePermissionAsync(IPermissionsCheckConfiguration configuration,
			int artifactTypeID, PermissionType artifactPermissions)
		{
			List<PermissionRef> permissionRefs = GetPermissionRefs(new ArtifactTypeIdentifier(artifactTypeID), artifactPermissions);

			bool userHasViewPermissions = false;
			try
			{
				IList<PermissionValue> permissions = await GetPermissionsForArtifactIdAsync(configuration.DestinationWorkspaceArtifactId, configuration.DestinationFolderArtifactId, permissionRefs)
					.ConfigureAwait(false);
				userHasViewPermissions = DoesUserHavePermissions(permissions);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex, "{PermissionCheck}: user does not have permission to access destination folder {FolderArtifactId} in destination workspace {ArtifactId}.",
					nameof(DestinationPermissionCheck), configuration.DestinationFolderArtifactId, configuration.DestinationWorkspaceArtifactId);
			}
			const string errorCode = "20.009";
			const string errorMessage = "User does not have permission to access the folder in the destination workspace or the folder does not exist.";

			return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage, errorCode);
		}
	}
}