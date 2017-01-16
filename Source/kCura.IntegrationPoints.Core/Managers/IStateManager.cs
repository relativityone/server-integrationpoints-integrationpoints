using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface IStateManager
	{
		/// <summary>
		///     Returns a set of booleans that convey the button state of the console buttons for all providers.
		/// </summary>
		/// <param name="providerType">Type of Integration Point Provider</param>
		/// <param name="hasJobsExecutingOrInQueue">If the current Integration Point has jobs running or queued up.</param>
		/// <param name="hasErrors">If the Integration Point has errors or not.</param>
		/// <param name="hasErrorViewPermissions">If the user can view Job History and Job History Error objects</param>
		/// <param name="hasStoppableJobs">If Integration Point has stoppable jobs</param>
		/// <param name="hasProfileAddPermission">If the user can add Integration Point Profile objects</param>
		/// <returns>
		///     A collection of booleans which explain the button state of the buttons on the console for the Relativity
		///     Provider.
		/// </returns>
		ButtonStateDTO GetButtonState(ProviderType providerType, bool hasJobsExecutingOrInQueue, bool hasErrors, bool hasErrorViewPermissions,
			bool hasStoppableJobs, bool hasProfileAddPermission);
	}
}