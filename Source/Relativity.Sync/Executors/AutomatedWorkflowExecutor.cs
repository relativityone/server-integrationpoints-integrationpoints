﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.AutomatedWorkflows.Services.Interfaces;
using Relativity.AutomatedWorkflows.Services.Interfaces.DataContracts.Triggers;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors.AutomatedWorkflow
{
	internal class AutomatedWorkflowExecutor : IExecutor<IAutomatedWorkflowTriggerConfiguration>
	{
		private readonly ISyncLog _logger;
		private readonly IServiceFactoryForAdmin _factoryForAdmin;

		internal AutomatedWorkflowExecutor(ISyncLog logger, IServiceFactoryForAdmin helper)
		{
			_logger = logger;
			_factoryForAdmin = helper;
		}

		public async Task<ExecutionResult> ExecuteAsync(IAutomatedWorkflowTriggerConfiguration configuration, CancellationToken token)
		{
			string state = configuration.SynchronizationExecutionResult.Status == ExecutionStatus.Completed ? "complete" : "complete-with-errors";
			
			_logger.LogInformation("For workspace artifact ID : {0} {1} trigger called with status {2}.", configuration.SourceWorkspaceArtifactId, configuration.TriggerName, state);
			try
			{
				SendTriggerBody body = new SendTriggerBody()
				{
					Inputs = new List<TriggerInput>
					{
						new TriggerInput()
						{
							ID = "rip-import",
							Value = "RelativityIntegrationPoints"
						}
					},
					State = state
				};
				using (IAutomatedWorkflowsService triggerProcessor = await _factoryForAdmin.CreateProxyAsync<IAutomatedWorkflowsService>().ConfigureAwait(false))
				{
					await triggerProcessor.SendTriggerAsync(configuration.SourceWorkspaceArtifactId, configuration.TriggerName, body).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				string message = "Error occured while executing trigger : {0} for workspace artifact ID : {1}";
				_logger.LogError(ex, message, configuration.TriggerName, configuration.SourceWorkspaceArtifactId);
			}
			_logger.LogInformation("For workspace : {0} trigger {1} finished sending.", configuration.SourceWorkspaceArtifactId, configuration.TriggerName);
			return await Task.FromResult(ExecutionResult.Success()).ConfigureAwait(false);
		}
	}
}