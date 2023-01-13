using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Permission;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Pipelines.Extensions;

namespace Relativity.Sync.Executors.PermissionCheck.DocumentPermissionChecks
{
    internal class DestinationDocumentPermissionCheck : DestinationPermissionCheckBase
    {
        private const string _MISSING_DESTINATION_RDO_PERMISSIONS =
            "User does not have permissions to view, edit, and add Documents in the destination workspace.";

        private const string _MISSING_DESTINATION_SAVED_SEARCH_ADD_PERMISSION =
            "User does not have permission to create saved searches in the destination workspace.";

        private const string _SOURCE_WORKSPACE_OBJECT_TYPE_NAME = "Relativity Source Case";
        private const string _SOURCE_JOB_OBJECT_TYPE_NAME = "Relativity Source Job";

        private readonly ISyncObjectTypeManager _syncObjectTypeManager;

        public DestinationDocumentPermissionCheck(
            IDestinationServiceFactoryForUser destinationServiceFactory,
            ISyncObjectTypeManager syncObjectTypeManager, IAPILog logger) : base(destinationServiceFactory, logger)
        {
            _syncObjectTypeManager = syncObjectTypeManager;
        }

        public override bool ShouldValidate(ISyncPipeline pipeline) => pipeline.IsDocumentPipeline();

        protected override async Task ValidateAsync(ValidationResult validationResult, IPermissionsCheckConfiguration configuration)
        {
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
        }

        private async Task<ValidationResult> ValidateFolderPermissionsUserHasArtifactInstancePermissionAsync(
            IPermissionsCheckConfiguration configuration,
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
                Logger.LogInformation(ex, "{PermissionCheck}: user does not have permission to access destination folder {FolderArtifactId} in destination workspace {ArtifactId}.",
                    nameof(DestinationPermissionCheckBase), configuration.DestinationFolderArtifactId, configuration.DestinationWorkspaceArtifactId);
            }

            const string errorCode = "20.009";
            const string errorMessage = "User does not have permission to access the folder in the destination workspace or the folder does not exist.";

            return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage, errorCode);
        }

        private async Task<ValidationResult> ValidateUserCanCreateTagsInDestinationWorkspaceAsync(IPermissionsCheckConfiguration configuration)
        {
            ValidationResult validationResult = new ValidationResult();

            if (!configuration.EnableTagging)
            {
                Logger.LogInformation("Tagging is disabled. Tags creation permission check in Destination Workspace will be skipped.");
                return validationResult;
            }

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
                int objectArtifactTypeID = await _syncObjectTypeManager.GetObjectTypeArtifactTypeIdAsync(configuration.DestinationWorkspaceArtifactId, objectTypeQueryResult.Objects.First().ArtifactID).ConfigureAwait(false);

                return await ValidateUserHasArtifactTypePermissionAsync(
                    configuration,
                    objectArtifactTypeID, new[] { PermissionType.View, PermissionType.Add },
                    insufficientPermissionsMessage).ConfigureAwait(false);
            }

            throw new ValidationException($"Cannot find Object Type: {objectTypeName} in Destination Workspace Artifact ID: {configuration.DestinationWorkspaceArtifactId}");
        }
    }
}
