﻿using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers
{
	public interface IOnClickEventConstructor
	{
		/// <summary>
		/// Creates the set of On Click Events used for the console buttons for the Relativity Provider.
		/// </summary>
		/// <param name="workspaceId">The current workspace ID.</param>
		/// <param name="integrationPointId">The current integration point's ID.</param>
		/// <param name="buttonStates">A DTO of button states for the Relativity Provider.</param>
		/// <returns>A DTO containing a set of strings for the On Click Events, for the Relativity Provider.</returns>
		RelativityOnClickEventDTO GetOnClickEventsForRelativityProvider(int workspaceId, int integrationPointId, RelativityButtonStateDTO buttonStates);

		/// <summary>
		/// Creates the set of On Click Events used for the console buttons for the Non-Relativity Providers.
		/// </summary>
		/// <param name="workspaceId">The current workspace ID.</param>
		/// <param name="integrationPointId">The current integration point's ID.</param>
		/// <param name="buttonStates">A DTO of button states for the Non-Relativity Provider.</param>
		/// <returns>A DTO containing a set of strings for the On Click Events, for the Non-Relativity Provider.</returns>
		OnClickEventDTO GetOnClickEvents(int workspaceId, int integrationPointId, ButtonStateDTO buttonStates);
	}
}
