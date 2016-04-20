using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Data.Factories
{
	/// <summary>
	/// Responsible for creating the necessary repository classes
	/// </summary>
	public interface IRepositoryFactory
	{
		/// <summary>
		/// Returns a class implementing the ISourceWorkspaceRepository interface
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id</param>
		/// <returns>A class implementing the ISourceWorkspaceRepository interface</returns>
		ISourceWorkspaceRepository GetSourceWorkspaceRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the ISourceWorkspaceJobHistoryRepository interface 
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id</param>
		/// <returns>A class implementing the ISourceWorkspaceJobHistoryRepository interface</returns>
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
		/// <returns>A class implementing the IJobHistoryRepository interface</returns>
		IJobHistoryRepository GetJobHistoryRepository();

		/// <summary>
		/// Returns a class implementing the IArtifactGuidRepository interface
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id</param>
		/// <returns>A class implementing the IArtifactGuidRepository interface</returns>
		IArtifactGuidRepository GetArtifactGuidRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the IFieldRepository interface 
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id</param>
		/// <returns>A class implementing the IFieldRepository interface</returns>
		IFieldRepository GetFieldRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the IObjectTypeRepository interface
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id</param>
		/// <returns>A class implementing the IObjectTypeRepository interface</returns>
		IObjectTypeRepository GetObjectTypeRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the ITabRepository interface
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id</param>
		/// <returns>A class implementing the ITabRepository interface</returns>
		ITabRepository GetTabRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the IDocumentRepository interface
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id</param>
		/// <returns>A class implementing the IDocumentRepository interface</returns>
		IDocumentRepository GetDocumentRepository(int workspaceArtifactId);
	}
}