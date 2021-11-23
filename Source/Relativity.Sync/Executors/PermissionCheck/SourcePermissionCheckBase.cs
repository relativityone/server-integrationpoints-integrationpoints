using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services;
using Relativity.Services.Permission;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Executors.PermissionCheck
{
	internal abstract class SourcePermissionCheckBase : PermissionCheckBase
	{
		private const string _JOB_HISTORY_TYPE_NO_ADD = "User does not have permission to add Job History RDOs in the source workspace.";
		private const string _OBJECT_TYPE_NO_ADD = "User does not have permission to add object types in the source workspace.";
		private const string _CONFIGURATION_TYPE_NO_ADD = "User does not have permission to the Configuration object type in the source workspace.";
		private const string _BATCH_OBJECT_TYPE_ERROR = "User does not have permission to the Batch object type in the source workspace.";
		private const string _PROGRESS_OBJECT_TYPE_ERROR = "User does not have permission to the Progress object type in the source workspace.";
		private const string _SOURCE_WORKSPACE_NO_EXPORT = "User does not have permission to export in the source workspace.";

		private const int _ALLOW_EXPORT_PERMISSION_ID = 159; // 159 is the artifact id of the "Allow Export" permission
		
		private readonly Guid _batchObjectTypeGuid = new Guid(SyncBatchGuids.SyncBatchObjectTypeGuid);
		private readonly Guid _progressObjectTypeGuid = new Guid(SyncProgressGuids.ProgressObjectTypeGuid);
		
		private readonly ISyncLog _logger;

		public SourcePermissionCheckBase(ISyncLog logger, ISourceServiceFactoryForUser sourceServiceFactory) : base(sourceServiceFactory)
		{
			_logger = logger;
		}

		public override async Task<ValidationResult> ValidateAsync(IPermissionsCheckConfiguration configuration)
		{
			var validationResult = new ValidationResult();

			validationResult.Add(await ValidateUserHasPermissionToAccessWorkspaceAsync(configuration).ConfigureAwait(false));
			validationResult.Add(await ValidatePermissionAsync(configuration, _ALLOW_EXPORT_PERMISSION_ID, _SOURCE_WORKSPACE_NO_EXPORT).ConfigureAwait(false));

			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, configuration.JobHistoryObjectTypeGuid, PermissionType.Add, _JOB_HISTORY_TYPE_NO_ADD).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, ArtifactType.ObjectType, PermissionType.Add, _OBJECT_TYPE_NO_ADD).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, new Guid(SyncRdoGuids.SyncConfigurationGuid), PermissionType.Edit, _CONFIGURATION_TYPE_NO_ADD).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, _batchObjectTypeGuid, new[] { PermissionType.Add, PermissionType.Edit, PermissionType.View },
				_BATCH_OBJECT_TYPE_ERROR).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, _progressObjectTypeGuid,
					new[] { PermissionType.Add, PermissionType.Edit, PermissionType.View }, _PROGRESS_OBJECT_TYPE_ERROR).ConfigureAwait(false));

			await ValidateAsync(validationResult, configuration).ConfigureAwait(false);
			
			return validationResult;
		}

		protected abstract Task ValidateAsync(ValidationResult validationResult, IPermissionsCheckConfiguration configuration);

		private async Task<ValidationResult> ValidateUserHasPermissionToAccessWorkspaceAsync(IPermissionsCheckConfiguration configuration)
		{
			var artifactTypeIdentifier = new ArtifactTypeIdentifier((int)ArtifactType.Case);
			List<PermissionRef> permissionRefs = GetPermissionRefs(artifactTypeIdentifier, PermissionType.View);

			bool userHasViewPermissions = false;
			try
			{
				IList<PermissionValue> permissions = await GetPermissionsForArtifactIdAsync(-1, configuration.SourceWorkspaceArtifactId, permissionRefs).ConfigureAwait(false);
				userHasViewPermissions = DoesUserHavePermissions(permissions);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex, "{PermissionCheck}: user does not have permission to view source workspace {ArtifactId}.", nameof(SourcePermissionCheckBase), configuration.SourceWorkspaceArtifactId);
			}

			const string errorMessage = "User does not have permission to access the source workspace.";
			return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage);
		}

		protected async Task<ValidationResult> ValidatePermissionAsync(IPermissionsCheckConfiguration configuration, int permissionId, string errorMessage)
		{
			List<PermissionRef> permissionRefs = GetPermissionRefs(permissionId);

			bool userHasViewPermissions = false;
			try
			{
				IList<PermissionValue> permissions = await GetPermissionsAsync(configuration.SourceWorkspaceArtifactId, permissionRefs).ConfigureAwait(false);
				userHasViewPermissions = DoesUserHavePermissions(permissions, permissionId);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex, "{PermissionCheck}: user does not have permission with ID {PermissionId} in the source workspace {ArtifactId}).", 
					nameof(SourcePermissionCheckBase), permissionId, configuration.SourceWorkspaceArtifactId);
			}

			return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage);
		}

		private Task<ValidationResult> ValidateUserHasArtifactTypePermissionAsync(IPermissionsCheckConfiguration configuration,
			Guid artifactTypeGuid, PermissionType artifactPermission, string errorMessage)
		{
			return ValidateUserHasArtifactTypePermissionAsync(configuration, artifactTypeGuid,
				new[] { artifactPermission }, errorMessage);
		}

		private Task<ValidationResult> ValidateUserHasArtifactTypePermissionAsync(IPermissionsCheckConfiguration configuration,
			Guid artifactTypeGuid, IEnumerable<PermissionType> artifactPermissions, string errorMessage)
		{
			var artifactTypeIdentifier = new ArtifactTypeIdentifier(artifactTypeGuid);
			return ValidateArtifactPermissionsAsync(configuration, artifactPermissions, errorMessage, artifactTypeIdentifier);
		}

		private Task<ValidationResult> ValidateUserHasArtifactTypePermissionAsync(IPermissionsCheckConfiguration configuration,
			ArtifactType artifactType, PermissionType artifactPermissions, string errorMessage)
		{
			return ValidateUserHasArtifactTypePermissionAsync(configuration, (int)artifactType, artifactPermissions,
				errorMessage);
		}
		
		protected Task<ValidationResult> ValidateUserHasArtifactTypePermissionAsync(IPermissionsCheckConfiguration configuration,
			int artifactType, PermissionType artifactPermissions, string errorMessage)
		{
			var artifactTypeIdentifier = new ArtifactTypeIdentifier(artifactType);
			return ValidateArtifactPermissionsAsync(configuration, new []{ artifactPermissions }, errorMessage, artifactTypeIdentifier);
		}

		private async Task<ValidationResult> ValidateArtifactPermissionsAsync(IPermissionsCheckConfiguration configuration,
			IEnumerable<PermissionType> artifactPermissions, string errorMessage, ArtifactTypeIdentifier artifactTypeIdentifier)
		{
			List<PermissionRef> permissionRefs = GetPermissionRefs(artifactTypeIdentifier, artifactPermissions);

			bool userHasViewPermissions = false;
			try
			{
				IList<PermissionValue> permissions =
					await GetPermissionsAsync(configuration.SourceWorkspaceArtifactId, permissionRefs)
						.ConfigureAwait(false);
				userHasViewPermissions = DoesUserHavePermissions(permissions);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex, "{PermissionCheck}: user does not have artifact type permission {ArtifactTypeIdentifier} in source workspace {ArtifactId}.", 
					nameof(SourcePermissionCheckBase), artifactTypeIdentifier, configuration.SourceWorkspaceArtifactId);
			}

			return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage);
		}
	}
}