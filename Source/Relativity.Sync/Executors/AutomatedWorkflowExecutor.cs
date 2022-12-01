using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.AutomatedWorkflows.SDK;
using Relativity.AutomatedWorkflows.SDK.V2.Models.Triggers;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
    internal class AutomatedWorkflowExecutor : IExecutor<IAutomatedWorkflowTriggerConfiguration>
    {
        private const string _ERROR_MESSAGE = "Error occured while executing Automated Workflows trigger: {0} for workspace artifact ID : {1}";

        private readonly IAPILog _logger;
        private readonly IAutomatedWorkflowsManager _automatedWorkflowsManager;

        public AutomatedWorkflowExecutor(IAPILog logger, IAutomatedWorkflowsManager automatedWorkflowsManager)
        {
            _logger = logger;
            _automatedWorkflowsManager = automatedWorkflowsManager;
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

                await _automatedWorkflowsManager.SendTriggerAsync(configuration.DestinationWorkspaceArtifactId, configuration.TriggerName, body).ConfigureAwait(false);
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
