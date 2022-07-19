using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.ExecutionConstrains
{
    internal sealed class AutomatedWorkflowExecutorConstrains : IExecutionConstrains<IAutomatedWorkflowTriggerConfiguration>
    {
        private const string _RELATIVITY_APPLICATION_NAME = "Relativity Application";
        private const string _AUTOMATED_WORKFLOWS_APPLICATION_NAME = "Automated Workflows";

        private readonly IDestinationServiceFactoryForAdmin _serviceFactory;
        private readonly IAPILog _logger;

        public AutomatedWorkflowExecutorConstrains(IDestinationServiceFactoryForAdmin serviceFactory, IAPILog logger)
        {
            _serviceFactory = serviceFactory;
            _logger = logger;
        }

        public async Task<bool> CanExecuteAsync(IAutomatedWorkflowTriggerConfiguration configuration, CancellationToken token)
        {
            return IsDocumentObjectFlow(configuration)
                   && configuration.SynchronizationExecutionResult != null
                   && (configuration.SynchronizationExecutionResult.Status == ExecutionStatus.Completed ||
                       configuration.SynchronizationExecutionResult.Status == ExecutionStatus.CompletedWithErrors)
                   && await IsAutomatedWorkflowsInstalledAsync(configuration.DestinationWorkspaceArtifactId)
                       .ConfigureAwait(false);

        }

        private bool IsDocumentObjectFlow(IAutomatedWorkflowTriggerConfiguration configuration)
        {
            return configuration.RdoArtifactTypeId == (int)ArtifactType.Document;
        }

        private async Task<bool> IsAutomatedWorkflowsInstalledAsync(int workspaceArtifactId)
        {
            try
            {
                using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
                using (IObjectTypeManager objectTypeManager = await _serviceFactory.CreateProxyAsync<IObjectTypeManager>().ConfigureAwait(false))
                {
                    QueryRequest relativityApplicationObjectTypeQueryRequest = new QueryRequest
                    {
                        ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.ObjectType },
                        Condition = $"'Name' == '{_RELATIVITY_APPLICATION_NAME}'"
                    };
                    QueryResultSlim relativityApplicationObjectTypeQueryResult = await objectManager.QuerySlimAsync(workspaceArtifactId, relativityApplicationObjectTypeQueryRequest, 0, 1).ConfigureAwait(false);

                    if (relativityApplicationObjectTypeQueryResult.ResultCount == 0)
                    {
                        throw new Exception($"The { _RELATIVITY_APPLICATION_NAME} object type wasn't found.");
                    }

                    int relativityApplicationObjectTypeArtifactId = relativityApplicationObjectTypeQueryResult.Objects[0].ArtifactID;
                    ObjectTypeResponse relativityApplicationObjectTypeMetadata = await objectTypeManager.ReadAsync(workspaceArtifactId, relativityApplicationObjectTypeArtifactId).ConfigureAwait(false);

                    QueryRequest automatedWorkflowsInstalledQueryRequest = new QueryRequest
                    {
                        ObjectType = new ObjectTypeRef { ArtifactTypeID = relativityApplicationObjectTypeMetadata.ArtifactTypeID },
                        Condition = $"'Name' == '{_AUTOMATED_WORKFLOWS_APPLICATION_NAME}'"
                    };
                    QueryResultSlim automatedWorkflowsInstalledQueryResult = await objectManager.QuerySlimAsync(workspaceArtifactId, automatedWorkflowsInstalledQueryRequest, 0, 0).ConfigureAwait(false);

                    _logger.LogInformation(_AUTOMATED_WORKFLOWS_APPLICATION_NAME + " installation status for workspace {workspaceArtifactId} is {installationStatus}.", workspaceArtifactId, automatedWorkflowsInstalledQueryResult.TotalCount > 0);

                    return automatedWorkflowsInstalledQueryResult.TotalCount > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Exception occurred when checking {_AUTOMATED_WORKFLOWS_APPLICATION_NAME} installation status.");
                return true;
            }
        }
    }
}
