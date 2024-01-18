using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Permission;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Pipelines.Extensions;

namespace Relativity.Sync.Executors.PermissionCheck.NonDocumentPermissionChecks
{
    internal class SourceNonDocumentPermissionCheck : SourcePermissionCheckBase
    {
        private string TransferredObjectNoView(int objectTypeArtifactId) => $"User does not have permission to view objects of type {objectTypeArtifactId} in the source workspace.";

        public SourceNonDocumentPermissionCheck(IAPILog logger, ISourceServiceFactoryForUser serviceFactoryForUser) : base(logger, serviceFactoryForUser)
        {
        }

        public override bool ShouldValidate(ISyncPipeline pipeline) => pipeline.IsNonDocumentPipeline();

        protected override async Task ValidateAsync(ValidationResult validationResult, IPermissionsCheckConfiguration configuration)
        {
            validationResult.Add(await ValidateUserHasArtifactTypePermissionAsync(configuration, configuration.RdoArtifactTypeId, PermissionType.View, TransferredObjectNoView(configuration.RdoArtifactTypeId)).ConfigureAwait(false));
        }
    }
}
