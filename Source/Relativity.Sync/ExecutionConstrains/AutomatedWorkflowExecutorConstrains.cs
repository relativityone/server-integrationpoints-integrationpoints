using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.ExecutionConstrains
{
	internal sealed class AutomatedWorkflowExecutorConstrains : IExecutionConstrains<IAutomatedWorkflowTriggerConfiguration>
	{
		private const int _RELATIVITY_APPLICATIONS_ARTIFACT_TYPE_ID = 1000014;
		private const string _AUTOMATED_WORKFLOWS_APPLICATION_NAME = "Automated Workflows";

		private readonly IDestinationServiceFactoryForAdmin _serviceFactory;
		private readonly ISyncLog _logger;

		public AutomatedWorkflowExecutorConstrains(IDestinationServiceFactoryForAdmin serviceFactory, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_logger = logger;
		}

		public async Task<bool> CanExecuteAsync(IAutomatedWorkflowTriggerConfiguration configuration, CancellationToken token)
		{
			return configuration.SynchronizationExecutionResult != null
			       && (configuration.SynchronizationExecutionResult.Status == ExecutionStatus.Completed || configuration.SynchronizationExecutionResult.Status == ExecutionStatus.CompletedWithErrors)
			       && await IsAutomatedWorkflowsInstalledAsync(configuration.DestinationWorkspaceArtifactId).ConfigureAwait(false);
		}

		private async Task<bool> IsAutomatedWorkflowsInstalledAsync(int workspaceArtifactId)
		{
			try
			{
				using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
				{
					QueryRequest automatedWorkflowsInstalledRequest = new QueryRequest
					{
						ObjectType = new ObjectTypeRef { ArtifactTypeID = _RELATIVITY_APPLICATIONS_ARTIFACT_TYPE_ID },
						Condition = $"'Name' == '{_AUTOMATED_WORKFLOWS_APPLICATION_NAME}'"
					};
					QueryResultSlim automatedWorkflowsInstalledResult = await objectManager.QuerySlimAsync(workspaceArtifactId, automatedWorkflowsInstalledRequest, 0, 0).ConfigureAwait(false);

					_logger.LogInformation(_AUTOMATED_WORKFLOWS_APPLICATION_NAME + " installation status for workspace {workspaceArtifactId} is {installationStatus}.", workspaceArtifactId, automatedWorkflowsInstalledResult.TotalCount > 0);

					return automatedWorkflowsInstalledResult.TotalCount > 0;
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