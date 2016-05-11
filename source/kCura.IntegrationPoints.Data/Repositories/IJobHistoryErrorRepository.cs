using System;
using System.Security.Claims;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IJobHistoryErrorRepository
	{
		/// <summary>
		/// Mass edits the Job History Errors 
		/// </summary>
		/// <param name="claimsPrincipal">A ClaimsPrincipal object that contains the identity of the user</param>
		/// <param name="numberOfDocs">The number of documents to be tagged</param>
		/// <param name="jobHistoryInstanceArtifactId">Artifact ID of the JobHistory RDO instance</param>
		/// <param name="sourceWorkspaceId">Artifact ID of the source workspace</param>
		/// <param name="errorStatus">The error status to update the errors to</param>
		/// <param name="tableSuffix">Unique suffix that is appended to the scratch table</param>
		void UpdateErrorStatuses(ClaimsPrincipal claimsPrincipal, int sourceWorkspaceId, Relativity.Client.Choice errorStatus, string tableSuffix);
	}
}
