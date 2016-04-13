using System;
using System.Collections.Generic;
using System.Data;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IScratchTableRepository : IDisposable
	{
		int Count { get; }

		void AddArtifactIdsIntoTempTable(List<int> artifactIds);

		void RemoveErrorDocument(string docIdentifier);

		IDataReader GetDocumentIdsDataReaderFromTable();

		void DeleteTable();

		string GetTempTableName();
	}
}