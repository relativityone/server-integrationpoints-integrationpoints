using System;
using System.Collections.Generic;
using System.Data;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IScratchTableRepository : IDisposable
	{
		/// <summary>
		/// Choice on whether to ignore errored documents when doing batch updating. Defaults to false.
		/// </summary>
		bool IgnoreErrorDocuments { get; set; }

		/// <summary>
		/// Temp table row count
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Allows you to add artifact ids into temp tables
		/// </summary>
		/// <param name="artifactIds">List of artifact ids to add to temp table</param>
		void AddArtifactIdsIntoTempTable(IList<int> artifactIds);

		/// <summary>
		/// Removes error documents from temp table list (goes in line with ignoring errored documents)
		/// </summary>
		/// <param name="docIdentifier">Identifier for the doc being removed from the list</param>
		void RemoveErrorDocument(string docIdentifier);

		/// <summary>
		/// Retrieve a datareader for reading from the temp table
		/// </summary>
		/// <returns></returns>
		IDataReader GetDocumentIdsDataReaderFromTable();

		/// <summary>
		/// Delete the temp table
		/// </summary>
		void DeleteTable();

		/// <summary>
		/// Dispose method for the ScratchTableRepository instance
		/// </summary>
		new void Dispose();

		/// <summary>
		/// Retrieve the temp table name for the ScratchTableRepository instance
		/// </summary>
		/// <returns></returns>
		string GetTempTableName();
	}
}