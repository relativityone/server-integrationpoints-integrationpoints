using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services;
using Relativity.Services.Permission;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors.PermissionCheck
{
	internal sealed class DestinationPermissionCheck : PermissionCheckBase
	{
		private const string _MISSING_DESTINATION_RDO_PERMISSIONS =
			"User does not have all required destination RDO permissions. Please make sure the user has view, edit, and add permissions for the destination RDO.";
		private const string _MISSING_DESTINATION_SAVED_SEARCH_ADD_PERMISSION =
			"User does not have permission to create saved searches in the destination workspace.";
		private const string _OBJECT_TYPE_NO_ADD = "User does not have permission to add object type in destination workspace Tag.";
		private const string _MISSING_DESTINATION_IMPORT =
			"User does not have permission to import in the destination workspace.";

		private const int _ALLOW_IMPORT_PERMISSION_ID = 158; // 158 is the artifact id of the "Allow Import" permission

		private readonly IDestinationServiceFactoryForUser _destinationServiceFactory;
		private readonly ISyncLog _logger;

		private readonly Guid _objectTypeGuid = new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC");

		public DestinationPermissionCheck(IDestinationServiceFactoryForUser destinationServiceFactory, ISyncLog logger)
		{
			_destinationServiceFactory = destinationServiceFactory;
			_logger = logger;
		}

		public override async Task<ValidationResult> ValidateAsync(IPermissionsCheckConfiguration configuration)
		{
			var validationResult = new ValidationResult();

			validationResult.Add(await ValidateDestinationWorkspaceUserHasPermissionToAccessWorkspaceAsync(configuration).ConfigureAwait(false));
			validationResult.Add(await ValidateDestinationWorkspaceUserCanImportHasPermissionAsync(configuration, _ALLOW_IMPORT_PERMISSION_ID, _MISSING_DESTINATION_IMPORT).ConfigureAwait(false));
			validationResult.Add(await ValidateDestinationWorkspaceUserHasArtifactTypePermissionsAsync(configuration,
				ArtifactType.Document, new[] { PermissionType.View, PermissionType.Add, PermissionType.Edit }, _MISSING_DESTINATION_RDO_PERMISSIONS).ConfigureAwait(false));
			validationResult.Add(await ValidateDestinationWorkspaceUserHasArtifactTypePermissionsAsync(configuration,
				ArtifactType.Search, new[] { PermissionType.Add }, _MISSING_DESTINATION_SAVED_SEARCH_ADD_PERMISSION).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, _objectTypeGuid, PermissionType.Add, _OBJECT_TYPE_NO_ADD).ConfigureAwait(false));
			validationResult.Add(await ValidateDestinationFolderPermissionsUserHasArtifactInstancePermissionAsync(configuration, ArtifactType.Document,
				PermissionType.Add).ConfigureAwait(false));
			validationResult.Add(await ValidateDestinationFolderPermissionsUserHasArtifactInstancePermissionAsync(configuration, ArtifactType.Folder,
				PermissionType.Add).ConfigureAwait(false));
			validationResult.Add(await ValidateDestinationFolderPermissionsUserHasArtifactInstancePermissionAsync(configuration, ArtifactType.Document,
				PermissionType.Delete).ConfigureAwait(false));

			return validationResult;
		}


		public async Task<ValidationResult> ValidateDestinationWorkspaceUserHasPermissionToAccessWorkspaceAsync(IPermissionsCheckConfiguration configuration)
		{
			var validationResult = new ValidationResult();

			var artifactTypeIdentifier = new ArtifactTypeIdentifier((int)ArtifactType.Case);
			List<PermissionRef> permissionRefs = GetPermissionRefs(artifactTypeIdentifier, PermissionType.View);

			bool userHasViewPermissions = false;
			try
			{
				IList<PermissionValue> permissions = await GetPermissionsAsync(_destinationServiceFactory, -1, configuration.DestinationWorkspaceArtifactId, permissionRefs).ConfigureAwait(false);
				userHasViewPermissions = DoesUserHavePermissions(permissions);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex, "Destination workspace: user has not permission to access workspace {WorkspaceArtifactID}.",
					configuration.DestinationWorkspaceArtifactId);
			}
			if (!userHasViewPermissions)
			{
				const string errorCode = "20.001";
				const string errorMessage = "User does not have sufficient permissions to access destination workspace. Contact your system administrator.";
				var validationMessage = new ValidationMessage(errorCode, errorMessage);
				validationResult.Add(validationMessage);
			}
			return validationResult;
		}

		public async Task<ValidationResult> ValidateDestinationWorkspaceUserCanImportHasPermissionAsync(
			IPermissionsCheckConfiguration configuration, int permissionId, string errorMessage)
		{
			var validationResult = new ValidationResult();

			List<PermissionRef> permissionRefs = GetPermissionRefs(permissionId);

			bool userHasViewPermissions = false;
			try
			{
				IList<PermissionValue> permissions = await GetPermissionsAsync(_destinationServiceFactory, configuration.DestinationWorkspaceArtifactId, permissionRefs).ConfigureAwait(false);
				userHasViewPermissions = DoesUserHavePermissions(permissions, permissionId);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex, "Destination workspace: user can not import and has not permission {WorkspaceArtifactID}.",
					configuration.DestinationWorkspaceArtifactId);
			}

			return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage);
		}

		private async Task<ValidationResult> ValidateUserHasArtifactTypePermissionAsync(IPermissionsCheckConfiguration configuration,
			Guid artifactTypeGuid, PermissionType artifactPermission, string errorMessage)
		{
			var artifactTypeIdentifier = new ArtifactTypeIdentifier(artifactTypeGuid);
			List<PermissionRef> permissionRefs = GetPermissionRefs(artifactTypeIdentifier, artifactPermission);

			bool userHasViewPermissions = false;
			try
			{
				IList<PermissionValue> permissions = await GetPermissionsAsync(_destinationServiceFactory, configuration.DestinationWorkspaceArtifactId, permissionRefs).ConfigureAwait(false);
				userHasViewPermissions = DoesUserHavePermissions(permissions);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex, "User does not have artifact type permission {WorkspaceArtifactID}.", configuration.SourceWorkspaceArtifactId);
			}

			return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage);
		}

		private async Task<ValidationResult> ValidateDestinationWorkspaceUserHasArtifactTypePermissionsAsync(IPermissionsCheckConfiguration configuration,
			ArtifactType artifactTypeIdentifier, IEnumerable<PermissionType> artifactPermissions, string errorMessage)
		{
			List<PermissionRef> permissionRefs = GetPermissionRefs(new ArtifactTypeIdentifier((int)artifactTypeIdentifier), artifactPermissions);

			bool userHasViewPermissions = false;

			try
			{
				IList<PermissionValue> permissions = await GetPermissionsAsync(_destinationServiceFactory,
					configuration.DestinationWorkspaceArtifactId, permissionRefs).ConfigureAwait(false);

				userHasViewPermissions = DoesUserHavePermissions(permissions);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex, "Destination workspace: user has not artifact type permission {WorkspaceArtifactID}.",
					configuration.DestinationWorkspaceArtifactId);
			}

			return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage);
		}

		private async Task<ValidationResult> ValidateDestinationFolderPermissionsUserHasArtifactInstancePermissionAsync(IPermissionsCheckConfiguration configuration,
			ArtifactType artifactTypeIdentifier, PermissionType artifactPermissions)
		{
			List<PermissionRef> permissionRefs = GetPermissionRefs(new ArtifactTypeIdentifier((int)artifactTypeIdentifier), artifactPermissions);

			var validationResult = new ValidationResult();
			bool userHasViewPermissions = false;

			try
			{
				IList<PermissionValue> permissions = await GetPermissionsAsync(_destinationServiceFactory, configuration.DestinationWorkspaceArtifactId, configuration.DestinationFolderArtifactId, permissionRefs)
					.ConfigureAwait(false);
				userHasViewPermissions = DoesUserHavePermissions(permissions);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex,
					"Destination Folder {DestinationFolderArtifactId}: user has not artifact instance permission {WorkspaceArtifactID}.",
					configuration.DestinationFolderArtifactId, configuration.DestinationWorkspaceArtifactId);
			}

			if (!userHasViewPermissions)
			{
				const string errorCode = "20.009";
				const string errorMessage = "Verify if a folder in destination workspace selected in the Integration Point exists or if a user has a proper permission to access it.";
				var validationMessage = new ValidationMessage(errorCode, errorMessage);
				validationResult.Add(validationMessage);
			}
			return validationResult;
		}
	}
}