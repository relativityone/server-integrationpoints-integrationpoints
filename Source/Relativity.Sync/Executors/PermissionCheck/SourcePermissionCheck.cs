﻿using System;
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
	internal sealed class SourcePermissionCheck : PermissionCheckBase
	{
		private const string _JOB_HISTORY_TYPE_NO_ADD = "User does not have permission to add Job History RDOs in the source workspace.";
		private const string _OBJECT_TYPE_NO_ADD = "User does not have permission to add object types in the source workspace.";
		private const string _CONFIGURATION_TYPE_NO_ADD = "User does not have permission to the Configuration object type in the source workspace.";
		private const string _BATCH_OBJECT_TYPE_ERROR = "User does not have permission to the Batch object type in the source workspace.";
		private const string _PROGRESS_OBJECT_TYPE_ERROR = "User does not have permission to the Progress object type in the source workspace.";
		private const string _SOURCE_WORKSPACE_NO_EXPORT = "User does not have permission to export in the source workspace.";
		private const string _SOURCE_WORKSPACE_NO_DOC_EDIT = "User does not have permission to edit Documents in this workspace.";

		private const int _ALLOW_EXPORT_PERMISSION_ID = 159; // 159 is the artifact id of the "Allow Export" permission
		private const int _EDIT_DOCUMENT_PERMISSION_ID = 45; // 45 is the artifact id of the "Edit Documents" permission

		private readonly Guid _jobHistory = new Guid("08f4b1f7-9692-4a08-94ab-b5f3a88b6cc9");
		private readonly Guid _batchObjectTypeGuid = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");
		private readonly Guid _progressObjectTypeGuid = new Guid("3D107450-DB18-4FE1-8219-73EE1F921ED9");


		private readonly ISyncLog _logger;

		public SourcePermissionCheck(ISyncLog logger, ISourceServiceFactoryForUser sourceServiceFactory) : base(sourceServiceFactory)
		{
			_logger = logger;
		}

		public override async Task<ValidationResult> ValidateAsync(IPermissionsCheckConfiguration configuration)
		{
			var validationResult = new ValidationResult();

			validationResult.Add(await ValidateUserHasPermissionToAccessWorkspaceAsync(configuration).ConfigureAwait(false));
			validationResult.Add(await ValidatePermissionAsync(configuration, _ALLOW_EXPORT_PERMISSION_ID, _SOURCE_WORKSPACE_NO_EXPORT).ConfigureAwait(false));
			validationResult.Add(await ValidatePermissionAsync(configuration, _EDIT_DOCUMENT_PERMISSION_ID, _SOURCE_WORKSPACE_NO_DOC_EDIT).ConfigureAwait(false));

			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, _jobHistory, PermissionType.Add, _JOB_HISTORY_TYPE_NO_ADD).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, ArtifactType.ObjectType, PermissionType.Add, _OBJECT_TYPE_NO_ADD).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, SyncConfigurationRdo.SyncConfigurationGuid, PermissionType.Edit, _CONFIGURATION_TYPE_NO_ADD).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, _batchObjectTypeGuid, new[] { PermissionType.Add, PermissionType.Edit, PermissionType.View },
				_BATCH_OBJECT_TYPE_ERROR).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, _progressObjectTypeGuid,
					new[] { PermissionType.Add, PermissionType.Edit, PermissionType.View }, _PROGRESS_OBJECT_TYPE_ERROR).ConfigureAwait(false));

			return validationResult;
		}

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
				_logger.LogInformation(ex, "{PermissionCheck}: user does not have permission to view source workspace {ArtifactId}.", nameof(SourcePermissionCheck), configuration.SourceWorkspaceArtifactId);
			}

			const string errorMessage = "User does not have permission to access the source workspace.";
			return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage);
		}

		private async Task<ValidationResult> ValidatePermissionAsync(IPermissionsCheckConfiguration configuration, int permissionId, string errorMessage)
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
					nameof(SourcePermissionCheck), permissionId, configuration.SourceWorkspaceArtifactId);
			}

			return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage);
		}

		private async Task<ValidationResult> ValidateUserHasArtifactTypePermissionAsync(IPermissionsCheckConfiguration configuration,
			Guid artifactTypeGuid, PermissionType artifactPermission, string errorMessage)
		{
			return await ValidateUserHasArtifactTypePermissionAsync(configuration, artifactTypeGuid,
				new[] { artifactPermission }, errorMessage).ConfigureAwait(false);
		}

		private async Task<ValidationResult> ValidateUserHasArtifactTypePermissionAsync(IPermissionsCheckConfiguration configuration,
			Guid artifactTypeGuid, IEnumerable<PermissionType> artifactPermissions, string errorMessage)
		{
			var artifactTypeIdentifier = new ArtifactTypeIdentifier(artifactTypeGuid);
			return await ValidateArtifactPermissions(configuration, artifactPermissions, errorMessage, artifactTypeIdentifier).ConfigureAwait(false);
		}

		private async Task<ValidationResult> ValidateUserHasArtifactTypePermissionAsync(IPermissionsCheckConfiguration configuration,
			ArtifactType artifactType, PermissionType artifactPermissions, string errorMessage)
		{
			var artifactTypeIdentifier = new ArtifactTypeIdentifier((int)artifactType);
			return await ValidateArtifactPermissions(configuration, new []{ artifactPermissions }, errorMessage, artifactTypeIdentifier).ConfigureAwait(false);
		}

		private async Task<ValidationResult> ValidateArtifactPermissions(IPermissionsCheckConfiguration configuration,
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
					nameof(SourcePermissionCheck), artifactTypeIdentifier, configuration.SourceWorkspaceArtifactId);
			}

			return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage);
		}
	}
}