using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Permission;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors.PermissionCheck
{
    internal abstract class DestinationPermissionCheckBase : PermissionCheckBase
    {
        protected readonly IAPILog Logger;

        public DestinationPermissionCheckBase(IDestinationServiceFactoryForUser destinationServiceFactory, IAPILog logger)
            : base(destinationServiceFactory)
        {
            Logger = logger;
        }

        public override async Task<ValidationResult> ValidateAsync(IPermissionsCheckConfiguration configuration)
        {
            var validationResult = new ValidationResult();

            validationResult.Add(await ValidateUserHasPermissionToAccessWorkspaceAsync(configuration).ConfigureAwait(false));

            validationResult.Add(await ValidateUserCanImportHasPermissionAsync(configuration).ConfigureAwait(false));

            await ValidateAsync(validationResult, configuration).ConfigureAwait(false);

            return validationResult;
        }

        protected abstract Task ValidateAsync(ValidationResult validationResult,
            IPermissionsCheckConfiguration configuration);

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
                Logger.LogInformation(ex, "{PermissionCheck}: user does not have permission to access workspace {ArtifactId}.", nameof(DestinationPermissionCheckBase), configuration.DestinationWorkspaceArtifactId);
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
                Logger.LogInformation(ex, "{PermissionCheck}: user does not have allow import permission in destination workspace {ArtifactId}.",
                    nameof(DestinationPermissionCheckBase), configuration.DestinationWorkspaceArtifactId);
            }
            const string errorMessage = "User does not have permission to import in the destination workspace.";
            return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage);
        }

        protected async Task<ValidationResult> ValidateUserHasArtifactTypePermissionAsync(IPermissionsCheckConfiguration configuration,
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
                Logger.LogInformation(ex, "{PermissionCheck}: user does not have artifact type {ArtifactTypeIdentifier} permission(s) in destination workspace {ArtifactId}.",
                    nameof(DestinationPermissionCheckBase), artifactTypeID, configuration.DestinationWorkspaceArtifactId);
            }
            return DoesUserHaveViewPermission(userHasViewPermissions, errorMessage);
        }
    }
}
