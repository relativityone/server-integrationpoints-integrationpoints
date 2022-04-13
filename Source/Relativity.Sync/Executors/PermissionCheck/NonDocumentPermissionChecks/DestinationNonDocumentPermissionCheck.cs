using Relativity.API;
using System.Threading.Tasks;
using Relativity.Services.Permission;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Pipelines.Extensions;

namespace Relativity.Sync.Executors.PermissionCheck.NonDocumentPermissionChecks
{
    internal class DestinationNonDocumentPermissionCheck : DestinationPermissionCheckBase
    {
        public DestinationNonDocumentPermissionCheck(IDestinationServiceFactoryForUser destinationServiceFactory, IAPILog logger) : base(destinationServiceFactory, logger)
        {
        }

        public override bool ShouldValidate(ISyncPipeline pipeline) => pipeline.IsNonDocumentPipeline();

        protected override async Task ValidateAsync(ValidationResult validationResult, IPermissionsCheckConfiguration configuration)
        {
			int destinationRdoArtifactTypeId = configuration.DestinationRdoArtifactTypeId;

			validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, destinationRdoArtifactTypeId, new []{ PermissionType.Add }, TransferredObjectNoAdd(destinationRdoArtifactTypeId)).ConfigureAwait(false));
            
            if (configuration.ImportOverwriteMode == ImportOverwriteMode.AppendOverlay ||
                configuration.ImportOverwriteMode == ImportOverwriteMode.OverlayOnly)
            {
                validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, destinationRdoArtifactTypeId, new []{ PermissionType.Edit }, TransferredObjectNoEdit(destinationRdoArtifactTypeId)).ConfigureAwait(false));
            }
        }

        private static string TransferredObjectNoAdd(int objectTypeArtifactId) => $"User does not have permission to add objects of type {objectTypeArtifactId} in the destination workspace.";
        private static string TransferredObjectNoEdit(int objectTypeArtifactId) => $"User does not have permission to edit objects of type {objectTypeArtifactId} in the destination workspace.";
    }
}
