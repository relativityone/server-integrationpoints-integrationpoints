using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface IStateManager
	{
		/// <summary>
		/// Returns a set of booleans that convey the button state of the console buttons for the Relativity provider.
		/// </summary>
		/// <param name="hasJobsExecutingOrInQueue">If the current Integration Point has jobs running or queued up.</param>
		/// <param name="hasErrors">If the Integration Point has errors or not.</param>
		/// <param name="hasViewPermissions">If the user can view Job History and Job History Error objects</param>
		/// <param name="hasStoppableJobs">If Integration Point has stoppable jobs</param>
		/// <returns>A collection of booleans which explain the button state of the buttons on the console for the Relativity Provider.</returns>
		RelativityButtonStateDTO GetRelativityProviderButtonState(bool hasJobsExecutingOrInQueue, bool hasErrors, bool hasViewPermissions, bool hasStoppableJobs);

		/// <summary>
		/// Returns a set of booleans that convey the button state of the console buttons for a non-Relativity provider.
		/// </summary>
		/// <param name="hasJobsExecutingOrInQueue">If Integration Point has jobs executing or queued up.</param>
		/// <param name="hasStoppableJobs">If Integration Point has stoppable jobs</param>
		/// <returns>A collection of booleans which explain the button state of the buttons on the console.</returns>
		ButtonStateDTO GetButtonState(bool hasJobsExecutingOrInQueue, bool hasStoppableJobs);
	}
}
