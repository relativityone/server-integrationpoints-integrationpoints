using System.Threading.Tasks;
using Relativity.Sync.AutomatedWorkflows.SDK.V2.Models.Triggers;

namespace Relativity.Sync.AutomatedWorkflows.SDK
{
	public class AutomatedWorkflowsManager : IAutomatedWorkflowsManager
	{
		public Task SendTriggerAsync(int workspaceId, string triggerId, SendTriggerBody triggerBody)
		{
			return Task.CompletedTask;
		}
	}
}
