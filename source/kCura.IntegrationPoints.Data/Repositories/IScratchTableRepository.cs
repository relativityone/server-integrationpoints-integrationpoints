﻿using System;
using System.Collections.Generic;
using System.Data;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IScratchTableRepository : IDisposable
	{
		bool IgnoreErrorDocuments { get; }

		int Count { get; }

		void AddArtifactIdsIntoTempTable(IList<int> artifactIds);

		void RemoveErrorDocument(string docIdentifier);

		IDataReader GetDocumentIdsDataReaderFromTable();

		void DeleteTable();

		string GetTempTableName();
	}
}