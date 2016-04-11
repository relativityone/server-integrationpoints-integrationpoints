using System;
using System.Collections.Generic;
using System.Data;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IScratchTableRepository : IDisposable
	{
		void AddArtifactIdsIntoTempTable(List<int> artifactIds);

		void RemoveErrorDocument(string docIdentifier);

		List<int> GetDocumentIdsFromTable();

		IDataReader GetDocumentIdsDataReaderFromTable();

		void DeleteTable();

		string GetTempTableName();
	}
}