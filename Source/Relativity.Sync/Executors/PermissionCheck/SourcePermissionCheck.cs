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
	internal sealed class SourcePermissionCheck : PermissionCheckBase
	{
		private const string _INTEGRATION_POINT_INSTANCE_NO_VIEW = "User does not have permission to view the Integration Point.";
		private const string _INTEGRATION_POINT_TYPE_NO_VIEW = "User does not have permission to view Integration Points.";
		private const string _JOB_HISTORY_TYPE_NO_ADD = "User does not have permission to add Job History RDOs.";
		private const string _SOURCE_PROVIDER_NO_VIEW = "User does not have permission to view Source Provider RDOs.";
		private const string _DESTINATION_PROVIDER_NO_VIEW = "User does not have permission to view Destination Provider RDOs.";
		private const string _SOURCE_PROVIDER_NO_INSTANCE_VIEW = "User does not have permission to view the Source Provider RDO.";
		private const string _SOURCE_WORKSPACE_NO_EXPORT = "User does not have permission to export in the source workspace.";
		private const string _SOURCE_WORKSPACE_NO_DOC_EDIT = "User does not have permission to edit documents in this workspace.";

		private const int _ALLOW_EXPORT_PERMISSION_ID = 159; // 159 is the artifact id of the "Allow Export" permission
		private const int _EDIT_DOCUMENT_PERMISSION_ID = 45; // 45 is the artifact id of the "Edit Documents" permission

		private readonly Guid JobHistory = new Guid("08f4b1f7-9692-4a08-94ab-b5f3a88b6cc9");
		private readonly Guid SourceProvider = new Guid("5be4a1f7-87a8-4cbe-a53f-5027d4f70b80");
		private readonly Guid DestinationProvider = new Guid("d014f00d-f2c0-4e7a-b335-84fcb6eae980");
		private readonly Guid IntegrationPoint = new Guid("03d4f67e-22c9-488c-bee6-411f05c52e01");

		private readonly ISyncLog _logger;
		private readonly ISourceServiceFactoryForUser _sourceServiceFactory;

		public SourcePermissionCheck(ISyncLog logger, ISourceServiceFactoryForUser sourceServiceFactory)
		{
			_logger = logger;
			_sourceServiceFactory = sourceServiceFactory;
		}

		public override async Task<ValidationResult> ValidateAsync(IPermissionsCheckConfiguration configuration, CancellationToken token)
		{
			var validationResult = new ValidationResult();

			validationResult.Add(await ValidateUserHasPermissionToAccessWorkspace(configuration).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactTypePermission(configuration, IntegrationPoint, PermissionType.View, _INTEGRATION_POINT_TYPE_NO_VIEW).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactInstancePermissions(configuration, IntegrationPoint, configuration.IntegrationPointArtifactId, PermissionType.View,
				_INTEGRATION_POINT_INSTANCE_NO_VIEW).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactTypePermission(configuration, JobHistory, PermissionType.Add, _JOB_HISTORY_TYPE_NO_ADD).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactTypePermission(configuration, SourceProvider, PermissionType.View, _SOURCE_PROVIDER_NO_VIEW).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactTypePermission(configuration, DestinationProvider, PermissionType.View, _DESTINATION_PROVIDER_NO_VIEW).ConfigureAwait(false));
			validationResult.Add(await ValidateUserHasArtifactInstancePermissions(configuration, SourceProvider, configuration.SourceProviderArtifactId, PermissionType.View,
				_SOURCE_PROVIDER_NO_INSTANCE_VIEW).ConfigureAwait(false));

			validationResult.Add(await ValidateSourceWorkspacePermission(configuration, _ALLOW_EXPORT_PERMISSION_ID, _SOURCE_WORKSPACE_NO_EXPORT).ConfigureAwait(false));
			validationResult.Add(await ValidateSourceWorkspacePermission(configuration, _EDIT_DOCUMENT_PERMISSION_ID, _SOURCE_WORKSPACE_NO_DOC_EDIT).ConfigureAwait(false));

			return validationResult;
		}

		private async Task<ValidationResult> ValidateUserHasPermissionToAccessWorkspace(IPermissionsCheckConfiguration configuration)
		{
			var artifactTypeIdentifier = new ArtifactTypeIdentifier((int)ArtifactType.Case);
			List<PermissionRef> permissionRefs = GetPermissionRefs(artifactTypeIdentifier, PermissionType.View);

			bool userHasViewPermissions = false;
			try
			{
				IList<PermissionValue> permissions = await GetPermissions(_sourceServiceFactory, -1, configuration.SourceWorkspaceArtifactId, permissionRefs).ConfigureAwait(false);
				userHasViewPermissions = DoesUserHavePermissions(permissions);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex, "User does not have permission to view workspace {WorkspaceArtifactID}.", configuration.SourceWorkspaceArtifactId);
			}

			const string errorMessage = "User does not have permission to access this workspace.";
			return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage);
		}

		private async Task<ValidationResult> ValidateUserHasArtifactTypePermission(IPermissionsCheckConfiguration configuration, Guid artifactTypeGuid, PermissionType artifactPermission, string errorMessage)
		{
			var artifactTypeIdentifier = new ArtifactTypeIdentifier(artifactTypeGuid);
			List<PermissionRef> permissionRefs = GetPermissionRefs(artifactTypeIdentifier, artifactPermission);

			bool userHasViewPermissions = false;
			try
			{
				IList<PermissionValue> permissions = await GetPermissions(_sourceServiceFactory, configuration.SourceWorkspaceArtifactId, permissionRefs).ConfigureAwait(false);
				userHasViewPermissions = DoesUserHavePermissions(permissions);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex, "User does not have artifact type permission {WorkspaceArtifactID}.", configuration.SourceWorkspaceArtifactId);
			}

			return DoesUserHaveViewPermission(userHasViewPermissions,errorMessage);
		}

		private async Task<ValidationResult> ValidateUserHasArtifactInstancePermissions(IPermissionsCheckConfiguration configuration,
			Guid artifactTypeGuid, int artifactId, PermissionType artifactPermission, string errorMessage)
		{
			var artifactTypeIdentifier = new ArtifactTypeIdentifier(artifactTypeGuid);
			List<PermissionRef> permissionRefs = GetPermissionRefs(artifactTypeIdentifier, artifactPermission);

			bool userHasViewPermissions = false;
			try
			{
				IList<PermissionValue> permissions = await GetPermissions(_sourceServiceFactory, configuration.SourceWorkspaceArtifactId, artifactId, permissionRefs).ConfigureAwait(false);
				userHasViewPermissions = DoesUserHavePermissions(permissions);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex, "User does not have artifact instance permission {ArtifactID}.", artifactId);
			}

			return DoesUserHaveViewPermission(userHasViewPermissions,errorMessage);
		}

		private async Task<ValidationResult> ValidateSourceWorkspacePermission(IPermissionsCheckConfiguration configuration, int permissionId, string errorMessage)
		{
			List<PermissionRef> permissionRefs = GetPermissionRefs(permissionId);

			bool userHasViewPermissions = false;
			try
			{
				IList<PermissionValue> permissions = await GetPermissions(_sourceServiceFactory, configuration.SourceWorkspaceArtifactId, permissionRefs).ConfigureAwait(false);
				userHasViewPermissions = DoesUserHavePermissions(permissions, permissionId);
			}
			catch (Exception ex)
			{
				_logger.LogInformation(ex, "User can not Export and has not permission {WorkspaceArtifactID}.", configuration.SourceWorkspaceArtifactId);
			}

			return DoesUserHaveViewPermission(userHasViewPermissions,errorMessage);
		}

		private static ValidationResult DoesUserHaveViewPermission(bool userHasViewPermissions,string errorMessage)
		{
			var validationResult = new ValidationResult();
			if (!userHasViewPermissions)
			{
				var validationMessage = new ValidationMessage(errorMessage);
				validationResult.Add(validationMessage);
			}

			return validationResult;
		}
	}
}