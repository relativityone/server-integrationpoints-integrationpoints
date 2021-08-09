using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.AutomatedWorkflows.Services.Interfaces;
using Relativity.AutomatedWorkflows.Services.Interfaces.DataContracts.Triggers;

namespace Relativity.Sync.Executors
{
	internal class AutomatedWorkflowExecutor : IExecutor<IAutomatedWorkflowTriggerConfiguration>
	{
		private const string _ERROR_MESSAGE = "Error occured while executing Automated Workflows trigger: {0} for workspace artifact ID : {1}";
		
		private readonly ISyncLog _logger;
		private readonly IDestinationServiceFactoryForAdmin _serviceFactory;

		public AutomatedWorkflowExecutor(ISyncLog logger, IDestinationServiceFactoryForAdmin serviceFactory)
		{
			_logger = logger;
			_serviceFactory = serviceFactory;
		}

		public async Task<ExecutionResult> ExecuteAsync(IAutomatedWorkflowTriggerConfiguration configuration, CompositeCancellationToken token)
		{
			try
			{
				string state = configuration.SynchronizationExecutionResult.Status == ExecutionStatus.Completed ? "complete" : "complete-with-errors";
				
				_logger.LogInformation("For workspace artifact ID : {0} {1} trigger called with status {2}.", configuration.DestinationWorkspaceArtifactId, configuration.TriggerName, state);
				
				SendTriggerBody body = new SendTriggerBody
				{
					Inputs = new List<TriggerInput>
					{
						new TriggerInput
						{
							ID = configuration.TriggerId,
							Value = configuration.TriggerValue
						}
					},
					State = state
				};
				
				using (IAutomatedWorkflowsService triggerProcessor = await _serviceFactory.CreateProxyAsync<IAutomatedWorkflowsService>().ConfigureAwait(false))
				{
					await triggerProcessor.SendTriggerAsync(configuration.DestinationWorkspaceArtifactId, configuration.TriggerName, body).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				
				_logger.LogError(ex, _ERROR_MESSAGE, configuration.TriggerName, configuration.DestinationWorkspaceArtifactId);
			}
			
			_logger.LogInformation("For workspace : {0} trigger {1} finished sending.", configuration.DestinationWorkspaceArtifactId, configuration.TriggerName);

			return ExecutionResult.Success();
		}
	}
}