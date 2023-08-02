using System.Threading.Tasks;
using Relativity.AutomatedWorkflows.SDK.V2.Models.Triggers;

namespace Relativity.AutomatedWorkflows.SDK
{
	public interface IAutomatedWorkflowsManager
	{
		Task SendTriggerAsync(int workspaceId, string triggerId, SendTriggerBody triggerBody);
	}
}
