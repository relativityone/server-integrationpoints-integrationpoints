using System;
using System.Collections.Generic;
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
			"User does not have permissions to view, edit, and add Documents in the destination workspace.";
		private const string _MISSING_DESTINATION_SAVED_SEARCH_ADD_PERMISSION =
			"User does not have permission to create saved searches in the destination workspace.";
		private const string _OBJECT_TYPE_NO_ADD = "User does not have permission to add object types in the destination workspace.";

		private readonly ISyncLog _logger;

		public DestinationPermissionCheck(IDestinationServiceFactoryForUser destinationServiceFactory, ISyncLog logger) : base(destinationServiceFactory)
		{
			_logger = logger;
		}

		public override async Task<ValidationResult> ValidateAsync(IPermissionsCheckConfiguration configuration)
		{
			var validationResult = new ValidationResult();
			validationResult.Add(await ValidateUserHasPermissionToAccessWorkspaceAsync(configuration).ConfigureAwait(false));
			validationResult.Add(await ValidateUserCanImportHasPermissionAsync(configuration).ConfigureAwait(false));

			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration,
				ArtifactType.Document, new[] { PermissionType.View, PermissionType.Add, PermissionType.Edit }, _MISSING_DESTINATION_RDO_PERMISSIONS).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration,
				ArtifactType.Search, new[] { PermissionType.Add }, _MISSING_DESTINATION_SAVED_SEARCH_ADD_PERMISSION).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, ArtifactType.ObjectType,
				new[] { PermissionType.Add }, _OBJECT_TYPE_NO_ADD).ConfigureAwait(false));
			validationResult.Add(await ValidateFolderPermissionsUserHasArtifactInstancePermissionAsync(configuration, ArtifactType.Document,
				PermissionType.Add).ConfigureAwait(false));
			validationResult.Add(await ValidateFolderPermissionsUserHasArtifactInstancePermissionAsync(configuration, ArtifactType.Folder,
				PermissionType.Add).ConfigureAwait(false));
			validationResult.Add(await ValidateFolderPermissionsUserHasArtifactInstancePermissionAsync(configuration, ArtifactType.Document,
				PermissionType.Delete).ConfigureAwait(false));

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
			const int permissionId = 158; // 158 is the artifact id of the "Allow Import" permission
			List<PermissionRef> permissionRefs = GetPermissionRefs(permissionId);
			bool userHasViewPermissions = false;
			try
			{
				IList<PermissionValue> permissions = await GetPermissionsAsync(configuration.DestinationWorkspaceArtifactId, permissionRefs).ConfigureAwait(false);
				userHasViewPermissions = DoesUserHavePermissions(permissions, permissionId);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex, "{PermissionCheck}: user does not have allow import permission in destination workspace {ArtifactId}.", 
					nameof(DestinationPermissionCheck), configuration.DestinationWorkspaceArtifactId);
			}
			const string errorMessage = "User does not have permission to import in the destination workspace.";
			return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage);
		}

		private async Task<ValidationResult> ValidateUserHasArtifactTypePermissionAsync(IPermissionsCheckConfiguration configuration,
			ArtifactType artifactTypeIdentifier, IEnumerable<PermissionType> artifactPermissions, string errorMessage)
		{
			var typeIdentifier = new ArtifactTypeIdentifier((int)artifactTypeIdentifier);
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
					nameof(DestinationPermissionCheck), artifactTypeIdentifier, configuration.DestinationWorkspaceArtifactId);
			}
			return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage);
		}

		private async Task<ValidationResult> ValidateFolderPermissionsUserHasArtifactInstancePermissionAsync(IPermissionsCheckConfiguration configuration,
			ArtifactType artifactTypeIdentifier, PermissionType artifactPermissions)
		{
			List<PermissionRef> permissionRefs = GetPermissionRefs(new ArtifactTypeIdentifier((int)artifactTypeIdentifier), artifactPermissions);

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