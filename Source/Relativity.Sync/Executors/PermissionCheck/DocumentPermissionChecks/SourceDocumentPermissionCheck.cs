using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Pipelines.Extensions;

namespace Relativity.Sync.Executors.PermissionCheck.DocumentPermissionChecks
{
    internal sealed class SourceDocumentPermissionCheck : SourcePermissionCheckBase
    {
        private const int _EDIT_DOCUMENT_PERMISSION_ID = 45; // 45 is the artifact id of the "Edit Documents" permission
        private const string _SOURCE_WORKSPACE_NO_DOC_EDIT = "User does not have permission to edit Documents in this workspace.";

        
        public SourceDocumentPermissionCheck(IAPILog logger, ISourceServiceFactoryForUser serviceFactoryForUser) : base(logger, serviceFactoryForUser)
        {
        }

        public override bool ShouldValidate(ISyncPipeline pipeline) => pipeline.IsDocumentPipeline();

        protected override async Task ValidateAsync(ValidationResult validationResult, IPermissionsCheckConfiguration configuration)
        {
            validationResult.Add(await ValidatePermissionAsync(configuration, _EDIT_DOCUMENT_PERMISSION_ID, _SOURCE_WORKSPACE_NO_DOC_EDIT).ConfigureAwait(false));
        }
    }
}
