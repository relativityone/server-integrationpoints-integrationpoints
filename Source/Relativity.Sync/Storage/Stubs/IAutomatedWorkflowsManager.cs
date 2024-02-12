using System.Threading.Tasks;
using Relativity.Sync.AutomatedWorkflows.SDK.V2.Models.Triggers;

namespace Relativity.Sync.AutomatedWorkflows.SDK
{
	/// <summary>
	/// Stubbed the original interface and registered a NO-OP implementation into this project.This will not only reduce the number of changes but makes future backports easier.
	/// </summary>
	public interface IAutomatedWorkflowsManager
	{
		Task SendTriggerAsync(int workspaceId, string triggerId, SendTriggerBody triggerBody);
	}
}
