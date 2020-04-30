using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.AutomatedWorkflows.Services.Interfaces;
using Relativity.AutomatedWorkflows.Services.Interfaces.DataContracts.Triggers;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal class AutomatedWorkflowExecutor : IExecutor<IAutomatedWorkflowTriggerConfiguration>
	{
		private readonly ISyncLog _logger;
		private readonly ISyncServiceManager _servicesMgr;

		public AutomatedWorkflowExecutor(ISyncLog logger, ISyncServiceManager servicesMgr)
		{
			_logger = logger;
			_servicesMgr = servicesMgr;
		}

		public async Task<ExecutionResult> ExecuteAsync(IAutomatedWorkflowTriggerConfiguration configuration, CancellationToken token)
		{
			try
			{
				string state = configuration.SynchronizationExecutionResult.Status == ExecutionStatus.Completed ? "complete" : "complete-with-errors";
				
				_logger.LogInformation("For workspace artifact ID : {0} {1} trigger called with status {2}.", configuration.DestinationWorkspaceArtifactId, configuration.TriggerName, state);
				
				SendTriggerBody body = new SendTriggerBody()
				{
					Inputs = new List<TriggerInput>
					{
						new TriggerInput()
						{
							ID = configuration.TriggerId,
							Value = configuration.TriggerValue
						}
					},
					State = state
				};
				
				using (IAutomatedWorkflowsService triggerProcessor = _servicesMgr.CreateProxy<IAutomatedWorkflowsService>(ExecutionIdentity.System))
				{
					await triggerProcessor.SendTriggerAsync(configuration.DestinationWorkspaceArtifactId, configuration.TriggerName, body).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				string message = "Error occured while executing trigger : {0} for workspace artifact ID : {1}";
				_logger.LogWarning(ex, message, configuration.TriggerName, configuration.DestinationWorkspaceArtifactId);
			}
			
			_logger.LogInformation("For workspace : {0} trigger {1} finished sending.", configuration.DestinationWorkspaceArtifactId, configuration.TriggerName);

			return ExecutionResult.Success();
		}
	}
}