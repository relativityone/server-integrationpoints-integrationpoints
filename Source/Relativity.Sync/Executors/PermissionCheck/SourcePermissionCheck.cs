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
	internal sealed class SourcePermissionCheck : PermissionCheckBase
	{
		private const string _JOB_HISTORY_TYPE_NO_ADD = "User does not have permission to add Job History RDOs.";
		private const string _OBJECT_TYPE_NO_ADD = "User does not have permission to add object type in source workspace Tag.";
		private const string _CONFIGURATION_TYPE_NO_ADD = "User does not have permission to configuration object type in source workspace.";
		private const string _BATCH_OBJECT_TYPE_ERROR = "User does not have permission to batch object type in source workspace.";
		private const string _PROGRESS_OBJECT_TYPE_ERROR = "User does not have permission to progress object type in source workspace.";
		private const string _SOURCE_WORKSPACE_NO_EXPORT = "User does not have permission to export in the source workspace.";
		private const string _SOURCE_WORKSPACE_NO_DOC_EDIT = "User does not have permission to edit documents in this workspace.";

		private const int _ALLOW_EXPORT_PERMISSION_ID = 159; // 159 is the artifact id of the "Allow Export" permission
		private const int _EDIT_DOCUMENT_PERMISSION_ID = 45; // 45 is the artifact id of the "Edit Documents" permission

		private readonly Guid JobHistory = new Guid("08f4b1f7-9692-4a08-94ab-b5f3a88b6cc9");
		private readonly Guid ObjectTypeGuid = new Guid("3F45E490-B4CF-4C7D-8BB6-9CA891C0C198");
		private readonly Guid BatchObjectTypeGuid = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");
		private readonly Guid ProgressObjectTypeGuid = new Guid("3D107450-DB18-4FE1-8219-73EE1F921ED9");
		private readonly Guid ConfigurationObjectTypeGuid = new Guid("3BE3DE56-839F-4F0E-8446-E1691ED5FD57");

		private readonly ISyncLog _logger;
		private readonly ISourceServiceFactoryForUser _sourceServiceFactory;

		public SourcePermissionCheck(ISyncLog logger, ISourceServiceFactoryForUser sourceServiceFactory)
		{
			_logger = logger;
			_sourceServiceFactory = sourceServiceFactory;
		}

		public override async Task<ValidationResult> ValidateAsync(IPermissionsCheckConfiguration configuration)
		{
			var validationResult = new ValidationResult();

			validationResult.Add(await ValidateUserHasPermissionToAccessWorkspaceAsync(configuration).ConfigureAwait(false));
			validationResult.Add(await ValidateSourceWorkspacePermissionAsync(configuration, _ALLOW_EXPORT_PERMISSION_ID, _SOURCE_WORKSPACE_NO_EXPORT).ConfigureAwait(false));
			validationResult.Add(await ValidateSourceWorkspacePermissionAsync(configuration, _EDIT_DOCUMENT_PERMISSION_ID, _SOURCE_WORKSPACE_NO_DOC_EDIT).ConfigureAwait(false));

			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, JobHistory, PermissionType.Add, _JOB_HISTORY_TYPE_NO_ADD).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, ObjectTypeGuid, PermissionType.Add, _OBJECT_TYPE_NO_ADD).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, BatchObjectTypeGuid, new[] {PermissionType.Add, PermissionType.Edit, PermissionType.View },
				_BATCH_OBJECT_TYPE_ERROR).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, ProgressObjectTypeGuid,
					new[] {PermissionType.Add, PermissionType.Edit, PermissionType.View}, _PROGRESS_OBJECT_TYPE_ERROR).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, ConfigurationObjectTypeGuid, PermissionType.Edit, _CONFIGURATION_TYPE_NO_ADD).ConfigureAwait(false));

			return validationResult;
		}

		private async Task<ValidationResult> ValidateUserHasPermissionToAccessWorkspaceAsync(IPermissionsCheckConfiguration configuration)
		{
			var artifactTypeIdentifier = new ArtifactTypeIdentifier((int)ArtifactType.Case);
			List<PermissionRef> permissionRefs = GetPermissionRefs(artifactTypeIdentifier, PermissionType.View);

			bool userHasViewPermissions = false;
			try
			{
				IList<PermissionValue> permissions = await GetPermissionsAsync(_sourceServiceFactory, -1, configuration.SourceWorkspaceArtifactId, permissionRefs).ConfigureAwait(false);
				userHasViewPermissions = DoesUserHavePermissions(permissions);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex, "User does not have permission to view workspace {WorkspaceArtifactID}.", configuration.SourceWorkspaceArtifactId);
			}

			const string errorMessage = "User does not have permission to access this workspace.";
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
				IList<PermissionValue> permissions = await GetPermissionsAsync(_sourceServiceFactory, configuration.SourceWorkspaceArtifactId, permissionRefs).ConfigureAwait(false);
				userHasViewPermissions = DoesUserHavePermissions(permissions);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex, "User does not have artifact type permission {WorkspaceArtifactID}.", configuration.SourceWorkspaceArtifactId);
			}

			return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage);
		}
		private async Task<ValidationResult> ValidateUserHasArtifactTypePermissionAsync(IPermissionsCheckConfiguration configuration,
			Guid artifactTypeGuid, IEnumerable<PermissionType> artifactPermissions, string errorMessage)
		{
			var artifactTypeIdentifier = new ArtifactTypeIdentifier(artifactTypeGuid);
			List<PermissionRef> permissionRefs = GetPermissionRefs(artifactTypeIdentifier, artifactPermissions);

			bool userHasViewPermissions = false;
			try
			{
				IList<PermissionValue> permissions = await GetPermissionsAsync(_sourceServiceFactory, configuration.SourceWorkspaceArtifactId, permissionRefs).ConfigureAwait(false);
				userHasViewPermissions = DoesUserHavePermissions(permissions);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex, "User does not have artifact type permission {WorkspaceArtifactID}.", configuration.SourceWorkspaceArtifactId);
			}

			return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage);
		}

		private async Task<ValidationResult> ValidateSourceWorkspacePermissionAsync(IPermissionsCheckConfiguration configuration, int permissionId, string errorMessage)
		{
			List<PermissionRef> permissionRefs = GetPermissionRefs(permissionId);

			bool userHasViewPermissions = false;
			try
			{
				IList<PermissionValue> permissions = await GetPermissionsAsync(_sourceServiceFactory, configuration.SourceWorkspaceArtifactId, permissionRefs).ConfigureAwait(false);
				userHasViewPermissions = DoesUserHavePermissions(permissions, permissionId);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex, "User can not Export and has not permission {WorkspaceArtifactID}.", configuration.SourceWorkspaceArtifactId);
			}

			return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage);
		}
	}
}