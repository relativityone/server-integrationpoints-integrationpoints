using System.Threading.Tasks;
using Relativity.Sync.Storage.V2.Models.Triggers;

namespace Relativity.Sync.Storage
{
	public class AutomatedWorkflowsManager : IAutomatedWorkflowsManager
	{
		public Task SendTriggerAsync(int workspaceId, string triggerId, SendTriggerBody triggerBody)
		{
			return Task.CompletedTask;
		}
	}
}
