﻿using System.Security.Claims;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IJobHistoryRepository
	{
		/// <summary>
		/// Mass Edits/tags the Documents that were pushed with the DestinationWorkspace instance
		/// </summary>
		/// <param name="claimsPrincipal">A ClaimsPrincipal object that contains the identity of the user</param>
		/// <param name="numberOfDocs">The number of documents to be tagged</param>
		/// <param name="jobHistoryInstanceArtifactId">Artifact ID of the JobHistory RDO instance</param>
		/// <param name="sourceWorkspaceId">Artifact ID of the source workspace</param>
		/// <param name="tableSuffix">Unique suffix that is appended to the scratch table</param>
		void TagDocsWithJobHistory(ClaimsPrincipal claimsPrincipal, int numberOfDocs, int jobHistoryInstanceArtifactId, int sourceWorkspaceId, string tableSuffix);

		/// <summary>
		/// Gets last Job History artifact id for a given Integration Point
		/// </summary>
		/// <param name="integrationPointArtifactId">Integration Point artifact id</param>
		/// <returns>Artifact id of the Job History object</returns>
		int GetLastJobHistoryArtifactId(int integrationPointArtifactId);
	}
}
