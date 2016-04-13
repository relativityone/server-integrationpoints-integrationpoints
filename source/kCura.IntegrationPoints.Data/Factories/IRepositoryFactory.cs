using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Data.Factories
{
	/// <summary>
	/// Reponsible for creating the necessary repository classes
	/// </summary>
	public interface IRepositoryFactory
	{
		/// <summary>
		/// Returns a class implementing the ISourceWorkspaceRepository interface
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id</param>
		/// <returns>A class impelemeting the ISourceWorkspaceRepository interface</returns>
		ISourceWorkspaceRepository GetSourceWorkspaceRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class impelemtning the ISourceWorkspaceJobHistoryRepository interface 
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id</param>
		/// <returns>A class impelemnting the ISourceWorkspaceJobHistoryRepository interface</returns>
		ISourceWorkspaceJobHistoryRepository GetSourceWorkspaceJobHistoryRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the ISourceJobRepository interface
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id</param>
		/// <returns>A class implementing the ISourceJobRepository interface</returns>
		ISourceJobRepository GetSourceJobRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the IWorkspaceRepository interface
		/// </summary>
		/// <returns>A class implementing the IWorkspaceRepository</returns>
		IWorkspaceRepository GetWorkspaceRepository();

		/// <summary>
		/// Returns a class implementing the IDestinationWorkspaceRepository interface
		/// </summary>
		/// <param name="sourceWorkspaceArtifactId">The source workspace artifact id</param>
		/// <param name="targetWorkspaceArtifactId">The target workspace artifact id</param>
		/// <returns>A class implementing the IDestinationWorkspaceRepository interface</returns>
		IDestinationWorkspaceRepository GetDestinationWorkspaceRepository(int sourceWorkspaceArtifactId, int targetWorkspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the IJobHistoryRepository interface
		/// </summary>
		/// <returns>A class implemeting the IJobHistoryRepository interface</returns>
		IJobHistoryRepository GetJobHistoryRepository();

		/// <summary>
		/// Returns a class implementing the IArtifactGuidRepository interface
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id</param>
		/// <returns>A class impelementing the IArtifactGuidRepository interface</returns>
		IArtifactGuidRepository GetArtifactGuidRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class impelmenting the IFieldRepository interface 
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id</param>
		/// <returns>A class impelmenting the IFieldRepository interface</returns>
		IFieldRepository GetFieldRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the IObjectTypeRepository interface
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id</param>
		/// <returns>A class implementing the IObjectTypeRepository interface</returns>
		IObjectTypeRepository GetObjectTypeRepository(int workspaceArtifactId);

		/// <summary>
		/// Returnes a class implementing the ITabRepository interface
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id</param>
		/// <returns>A class implementing the ITabRepository interface</returns>
		ITabRepository GetTabRepository(int workspaceArtifactId);
	}
}