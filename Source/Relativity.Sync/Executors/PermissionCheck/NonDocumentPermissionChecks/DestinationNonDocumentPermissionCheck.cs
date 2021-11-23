using System.Collections.Generic;
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
        public DestinationNonDocumentPermissionCheck(IDestinationServiceFactoryForUser destinationServiceFactory, ISyncLog logger) : base(destinationServiceFactory, logger)
        {
        }

        public override bool ShouldValidate(ISyncPipeline pipeline) => pipeline.IsNonDocumentPipeline();

        protected override async Task ValidateAsync(ValidationResult validationResult, IPermissionsCheckConfiguration configuration)
        {
            validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, configuration.RdoArtifactTypeId,new []{ PermissionType.Add }, TransferredObjectNoAdd(configuration.RdoArtifactTypeId)).ConfigureAwait(false));
            
            if (configuration.ImportOverwriteMode == ImportOverwriteMode.AppendOverlay ||
                configuration.ImportOverwriteMode == ImportOverwriteMode.OverlayOnly)
            {
                validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, configuration.RdoArtifactTypeId,new []{ PermissionType.Edit }, TransferredObjectNoEdit(configuration.RdoArtifactTypeId)).ConfigureAwait(false));
            }
        }

        private static string TransferredObjectNoAdd(int objectTypeArtifactId) => $"User does not have permission to add objects of type {objectTypeArtifactId} in the destination workspace.";
        private static string TransferredObjectNoEdit(int objectTypeArtifactId) => $"User does not have permission to Edit objects of type {objectTypeArtifactId} in the destination workspace.";
    }
}