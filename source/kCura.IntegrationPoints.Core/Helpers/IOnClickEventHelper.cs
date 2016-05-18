using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Helpers
{
	public interface IOnClickEventHelper
	{
		/// <summary>
		/// Creates the set of On Click Events used for the console buttons for the Relativity Provider.
		/// </summary>
		/// <param name="workspaceId">The current workspace ID.</param>
		/// <param name="integrationPointId">The current integration point's ID.</param>
		/// <param name="buttonStates">A DTO of button states for the Relativity Provider.</param>
		/// <returns>A DTO containing a set of strings for the On Click Events, for the Relativity Provider.</returns>
		OnClickEventDTO GetOnClickEventsForRelativityProvider(int workspaceId, int integrationPointId, ButtonStateDTO buttonStates);
	}
}
