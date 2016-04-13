using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	/// Responsible for handling Artifact Guids
	/// </summary>
	public interface IArtifactGuidRepository
	{
		/// <summary>
		/// Inserts an Artifact Id and Artifact Guid pair into the ArtifactGuid table
		/// </summary>
		/// <param name="artifactId">The artifact id for the entry</param>
		/// <param name="guid">The artifact guid for the entry</param>
		void InsertArtifactGuidForArtifactId(int artifactId, Guid guid);

		/// <summary>
		/// Inserts multiple artifact id and guid pairs into the ArtifactGuid table
		/// </summary>
		/// <param name="guidToIdDictionary">A dictionary of guid and artifact id pairings to insert</param>
		void InsertArtifactGuidsForArtifactIds(IDictionary<Guid, int> guidToIdDictionary);

		/// <summary>
		/// Checks to see if the given guids exist in the ArtifactGuid table
		/// </summary>
		/// <param name="guids">The guids to search for</param>
		/// <returns>A dictionary of field guids and a value of <code>TRUE</code> or <code>FALSE</code> noting if the guid exists</returns>
		IDictionary<Guid, bool> GuidsExist(IEnumerable<Guid> guids);

		/// <summary>
		/// Checks to see if a single guid exists in the ArtifactGuid table
		/// </summary>
		/// <param name="guid">The guid to search for</param>
		/// <returns><code>TRUE</code> if the guid was found, <code>FALSE</code> otherwise</returns>
		bool GuidExists(Guid guid);
	}
}