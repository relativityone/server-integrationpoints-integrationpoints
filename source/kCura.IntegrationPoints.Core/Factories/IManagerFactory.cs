using kCura.IntegrationPoints.Core.Managers;

namespace kCura.IntegrationPoints.Core.Factories
{
	/// <summary>
	/// Manager factory is responsible for creation of specific managers.
	/// </summary>
	public interface IManagerFactory
	{
		/// <summary>
		/// Creates Artifact GUID manager
		/// </summary>
		/// <param name="contextContainer">Container containing necessary contexts</param>
		/// <returns>Artifact GUID manager</returns>
		IArtifactGuidManager CreateArtifactGuidManager(IContextContainer contextContainer);

		/// <summary>
		/// Creates field manager.
		/// </summary>
		/// <param name="contextContainer">Container containing necessary contexts</param>
		/// <returns>Field manager</returns>
		IFieldManager CreateFieldManager(IContextContainer contextContainer);

		/// <summary>
		/// Creates integration point manager
		/// </summary>
		/// <param name="contextContainer">Container containing necessary contexts</param>
		/// <returns>Integration point manager</returns>
		IIntegrationPointManager CreateIntegrationPointManager(IContextContainer contextContainer);

		/// <summary>
		/// Creates Job History manager
		/// </summary>
		/// <param name="contextContainer">Container containing necessary contexts</param>
		/// <returns>Job History manager</returns>
		IJobHistoryManager CreateJobHistoryManager(IContextContainer contextContainer);

		/// <summary>
		/// Creates Job History Error manager
		/// </summary>
		/// <param name="contextContainer">Container containing necessary contexts</param>
		/// <returns>Job History Error manager</returns>
		IJobHistoryErrorManager CreateJobHistoryErrorManager(IContextContainer contextContainer);

		/// <summary>
		/// Creates Object Type manager
		/// </summary>
		/// <param name="contextContainer">Container containing necessary contexts</param>
		/// <returns>Object Type manager</returns>
		IObjectTypeManager CreateObjectTypeManager(IContextContainer contextContainer);

		/// <summary>
		/// Creates Queue Manager
		/// </summary>
		/// <param name="contextContainer">Container containing necessary contexts</param>
		/// <returns>A queue manager</returns>
		IQueueManager CreateQueueManager(IContextContainer contextContainer);
		
		/// <summary>
		/// Create State manager.
		/// </summary>
		/// <returns>State manager (for console buttons)</returns>
		IStateManager CreateStateManager();
		
		/// <summary>
		/// Creates source provider manager
		/// </summary>
		/// <param name="contextContainer">Container containing necessary contexts</param>
		/// <returns>Source provider manager</returns>
		ISourceProviderManager CreateSourceProviderManager(IContextContainer contextContainer);

	}
}
