namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IJobHistoryRepository
	{
		/// <summary>
		/// Gets last finished Job History artifact id for a given Integration Point
		/// </summary>
		/// <param name="integrationPointArtifactId">Integration Point artifact id</param>
		/// <returns>Artifact id of the most recent finished Job History objects</returns>
		int GetLastJobHistoryArtifactId(int integrationPointArtifactId);

		/// <summary>
		/// Gets the stoppable Job History artifact ids for a given Integration Point.
		/// </summary>
		/// <param name="integrationPointArtifactId">The parent Integration Point artifact id.</param>
		/// <returns>The artifact ids of the job histories that can be stopped.</returns>
		int[] GetStoppableJobHistoryArtifactIds(int integrationPointArtifactId);
	}
}
