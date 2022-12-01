using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services;
using Relativity.Services.Permission;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.Executors.PermissionCheck
{
    internal abstract class PermissionCheckBase : IPermissionCheck
    {
        protected readonly IProxyFactory _proxyFactory;

        protected PermissionCheckBase(IProxyFactory proxyFactory)
        {
            _proxyFactory = proxyFactory;
        }

        public abstract Task<ValidationResult> ValidateAsync(IPermissionsCheckConfiguration configuration);

        public abstract bool ShouldValidate(ISyncPipeline pipeline);

        protected async Task<IList<PermissionValue>> GetPermissionsForArtifactIdAsync(int workspaceArtifactId, int artifactId, List<PermissionRef> permissionRefs)
        {
            using (var permissionManager = await _proxyFactory.CreateProxyAsync<IPermissionManager>().ConfigureAwait(false))
            {
                IList<PermissionValue> permissionValues = await permissionManager.GetPermissionSelectedAsync(workspaceArtifactId, permissionRefs, artifactId).ConfigureAwait(false);
                return permissionValues;
            }
        }

        protected async Task<IList<PermissionValue>> GetPermissionsAsync(int workspaceArtifactId, List<PermissionRef> permissionRefs)
        {
            using (var permissionManager = await _proxyFactory.CreateProxyAsync<IPermissionManager>().ConfigureAwait(false))
            {
                IList<PermissionValue> permissionValues = await permissionManager.GetPermissionSelectedAsync(workspaceArtifactId, permissionRefs).ConfigureAwait(false);
                return permissionValues;
            }
        }

        protected static List<PermissionRef> GetPermissionRefs(int permissionId)
        {
            var permissionRefs = new List<PermissionRef>
            {
                new PermissionRef { PermissionID = permissionId }
            };
            return permissionRefs;
        }

        protected static List<PermissionRef> GetPermissionRefs(ArtifactTypeIdentifier artifactTypeIdentifier, PermissionType artifactPermission)
        {
            return GetPermissionRefs(artifactTypeIdentifier, new[] { artifactPermission });
        }

        protected static List<PermissionRef> GetPermissionRefs(ArtifactTypeIdentifier artifactTypeIdentifier, IEnumerable<PermissionType> permissionTypes)
        {
            List<PermissionRef> permissionRefs = permissionTypes.Select(permissionType => new PermissionRef
            {
                ArtifactType = artifactTypeIdentifier,
                PermissionType = permissionType
            }).ToList();

            return permissionRefs;
        }

        protected static bool DoesUserHavePermissions(IList<PermissionValue> permissionValues)
        {
            bool userHasPermissions = false;
            if (permissionValues != null && permissionValues.Any())
            {
                userHasPermissions = permissionValues.All(x => x.Selected);
            }

            return userHasPermissions;
        }

        protected static bool DoesUserHavePermissions(IList<PermissionValue> permissionValues, int permissionId)
        {
            bool userHasPermissions = false;
            if (permissionValues != null && permissionValues.Any())
            {
                PermissionValue hasPermissionValue = permissionValues.First();
                userHasPermissions = hasPermissionValue.Selected && hasPermissionValue.PermissionID == permissionId;
            }

            return userHasPermissions;
        }

        protected static ValidationResult DoesUserHaveViewPermission(bool userHasViewPermissions, string errorMessage, string errorCode = null)
        {
            var validationResult = new ValidationResult();
            if (!userHasViewPermissions)
            {
                ValidationMessage validationMessage = (errorCode == null) ? new ValidationMessage(errorMessage) : new ValidationMessage(errorCode, errorMessage);
                validationResult.Add(validationMessage);
            }

            return validationResult;
        }
    }
}
