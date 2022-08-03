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
        int GetCount();

        /// <summary>
        /// Allows you to add artifact ids into temp tables
        /// </summary>
        /// <param name="artifactIds">Collection of artifact ids to add to temp table</param>
        void AddArtifactIdsIntoTempTable(ICollection<int> artifactIds);

        /// <summary>
        /// Creates a copy of the current scratch table repository
        /// </summary>
        /// <param name="newTempTablePrefix">Temp table prefix for the new table</param>
        /// <returns>Scratch table repository for the new copied table</returns>
        IScratchTableRepository CopyTempTable(string newTempTablePrefix);

        /// <summary>
        /// Removes error documents from temp table list (goes in line with ignoring errored documents)
        /// </summary>
        /// <param name="documentIDs">Identifiers for the documents being removed from the scratch table</param>
        void RemoveErrorDocuments(ICollection<string> documentIDs);

        /// <summary>
        /// Retrieve a datareader for reading from the temp table
        /// </summary>
        /// <returns></returns>
        IDataReader GetDocumentIDsDataReaderFromTable();

        /// <summary>
        /// Delete the temp table
        /// </summary>
        void DeleteTable();

        /// <summary>
        /// Retrieve artifact IDs from scratch table with offset. The artifacts are in descending order
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns>Document IDs from scratch table</returns>
        IEnumerable<int> ReadArtifactIDs(int offset, int size);

        /// <summary>
        /// Retrieve the temp table name for the ScratchTableRepository instance
        /// </summary>
        /// <returns></returns>
        string GetTempTableName();

        /// <summary>
        /// Returns schemaless resource database prepend string
        /// </summary>
        /// <returns></returns>
        string GetSchemalessResourceDataBasePrepend();

        /// <summary>
        /// Returns resource database prepend string
        /// </summary>
        /// <returns></returns>
        string GetResourceDBPrepend();
    }
}