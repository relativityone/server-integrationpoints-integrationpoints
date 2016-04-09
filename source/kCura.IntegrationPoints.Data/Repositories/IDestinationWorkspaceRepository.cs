namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IDestinationWorkspaceRepository
	{
		/// <summary>
		/// Queries to see if a Destination Workspace RDO instance exists for the target workspace
		/// </summary>
		/// <returns>-1 if no instance exists, Artifact ID of instance otherwise</returns>
		int? QueryDestinationWorkspaceRdoInstance();

		/// <summary>
		/// Creates an instance of a Destination Workspace RDO
		/// </summary>
		int? CreateDestinationWorkspaceRdoInstance();

		/// <summary>
		/// Mass Edits/tags the Documents that were pushed with the DestinationWorkspace instance
		/// </summary>
		/// <param name="numberOfDocs">The number of documents to tag</param>
		/// <param name="destinationWorkspaceInstanceId">Artifact ID of the DestinationWorkspace RDO instance</param>
		/// <param name="tableSuffix">Unique suffix that is appended to the scratch table</param>
		/// <param name="sourceWorkspaceId">Artifact ID of the source workspace</param>
		void TagDocsWithDestinationWorkspace(int numberOfDocs, int? destinationWorkspaceInstanceId, string tableSuffix,
			int sourceWorkspaceId);

		/// <summary>
		/// Links the multi-object fields on DestinationWorkspace and JobHistory objects
		/// </summary>
		/// <param name="destinationWorkspaceInstanceId">Artifact ID of the DestinationWorkspace RDO instance</param>
		/// <param name="jobHistoryInstanceId">Artifact ID of the JobHistory RDO instance</param>
		void LinkDestinationWorkspaceToJobHistory(int? destinationWorkspaceInstanceId, int jobHistoryInstanceId);
	}
}
