using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers
{
    public interface IOnClickEventConstructor
    {
        /// <summary>
        ///     Creates the set of On Click Events used for the console buttons for all provider.
        /// </summary>
        /// <param name="workspaceId">The current workspace ID.</param>
        /// <param name="integrationPointId">The current integration point's ID.</param>
        /// <param name="integrationPointName">The current integration point's name.</param>
        /// <param name="buttonStates">A DTO of button states for the Relativity Provider.</param>
        /// <returns>A DTO containing a set of strings for the On Click Events, for the Relativity Provider.</returns>
        OnClickEventDTO GetOnClickEvents(int workspaceId, int integrationPointId, string integrationPointName, ButtonStateDTO buttonStates);
    }
}
