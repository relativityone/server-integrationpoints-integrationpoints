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
		/// <param name="permissionSuccess">The status of whether or not the user has permission to run an Integration Point job.</param>
		/// <param name="hasErrors">If the Integration Point has errors or not.</param>
		/// <returns>A collection of booleans which explain the button state of the three console buttons.</returns>
		ButtonStateDTO GetButtonState(int workspaceId, int integrationPointId, bool permissionSuccess, bool hasErrors);
	}
}
