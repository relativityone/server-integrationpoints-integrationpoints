namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IJobHistoryRepository
	{
		/// <summary>
		/// Mass Edits/tags the Documents that were pushed with the DestinationWorkspace instance
		/// </summary>
		/// <param name="numberOfDocs">The number of documents to be tagged</param>
		/// <param name="jobHistoryInstanceArtifactId">Artifact ID of the JobHistory RDO instance</param>
		/// <param name="sourceWorkspaceId">Artifact ID of the source workspace</param>
		/// <param name="tableSuffix">Unique suffix that is appended to the scratch table</param>
		void TagDocsWithJobHistory(int numberOfDocs, int jobHistoryInstanceArtifactId, int sourceWorkspaceId, string tableSuffix);
	}
}
