using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Sync.Configuration;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.Executors.Validation
{
    internal class SnapshotValidator : IValidator
    {
        private readonly IValidationConfiguration _configuration;
        private readonly ISyncServiceManager _serviceManager;

        public SnapshotValidator(IValidationConfiguration configuration, ISyncServiceManager serviceManager)
        {
            _configuration = configuration;
            _serviceManager = serviceManager;
        }
        
        public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
        {
            var result = new ValidationResult();

            if (!await CheckSnapshotExistsAsync(configuration.SnapshotId, configuration.SourceWorkspaceArtifactId).ConfigureAwait(false))
            {
                result.Add($"Snapshot with Id [{configuration.SnapshotId}] does not exist in workspace {configuration.SourceWorkspaceArtifactId}");
            }
            
            return result;
        }

        private async Task<bool> CheckSnapshotExistsAsync(Guid? snapshotId, int workspaceId)
        {
            if (snapshotId == null)
            {
                return false;
            }

            using (IObjectManager objectManager = _serviceManager.CreateProxy<IObjectManager>(ExecutionIdentity.System))
            {
                return (await objectManager.RetrieveResultsBlockFromExportAsync(workspaceId, snapshotId.Value, 1, 0).ConfigureAwait(false)) != null;
            }
        }

        public bool ShouldValidate(ISyncPipeline _) => _configuration.Resuming;
    }
}