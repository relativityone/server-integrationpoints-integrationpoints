namespace kCura.IntegrationPoints.Core.Managers
{
	public interface IJobHistoryManager
	{
		/// <summary>
		/// Gets last Job History artifact id for a given Integration Point
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace artifact id</param>
		/// <param name="integrationPointArtifactId">Integration Point artifact id</param>
		/// <returns>Artifact id of the Job History object</returns>
		int GetLastJobHistoryArtifactId(int workspaceArtifactId, int integrationPointArtifactId);
	}
}
