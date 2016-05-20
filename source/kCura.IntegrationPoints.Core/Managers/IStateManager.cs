using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface IStateManager
	{
		/// <summary>
		/// Returns a set of booleans that convey the button state of the three console buttons for the Relativity provider.
		/// </summary>
		/// <param name="workspaceId">The Artifact ID of the current workspace.</param>
		/// <param name="integrationPointId">The Artifact ID of the current integration point.</param>
		/// <param name="hasJobsExecutingOrInQueue">If the current Integration Point has jobs running or queued up.</param>
		/// <param name="hasErrors">If the Integration Point has errors or not.</param>
		/// <returns>A collection of booleans which explain the button state of the three console buttons.</returns>
		ButtonStateDTO GetButtonState(int workspaceId, int integrationPointId, bool hasJobsExecutingOrInQueue, bool hasErrors);
	}
}
