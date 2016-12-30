using System;
using System.Linq;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Services
{
	/// <summary>
	/// Service handling verification and creation of data transfer job's folders
	/// </summary>
	public interface IDataTransferLocationService
	{
		/// <summary>
		/// Creates all necessary folders for all Integration Points Types
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace ID</param>
		void CreateForAllTypes(int workspaceArtifactId);

		/// <summary>
		/// Returns root path for Integration Point Type
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace ID</param>
		/// <param name="integrationPointTypeArtifactId">Integration Point Type</param>
		/// <returns>Path as string</returns>
		string GetLocationFor(int workspaceArtifactId, Guid integrationPointTypeArtifactId);

		/// <summary>
		/// Verifies and prepares all necessary folders
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace ID</param>
		/// <param name="path">Path to be used</param>
		/// <returns>Verified path</returns>
		string VerifyAndPrepare(int workspaceArtifactId, string path);
	}
}