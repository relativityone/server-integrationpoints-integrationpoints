using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Data.Repositories;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
	public class FakeScratchTableRepository : IScratchTableRepository
	{
		private readonly InMemoryDatabase _db;

		public FakeScratchTableRepository(InMemoryDatabase db, int workspaceArtifactID, string tablePrefix, string tableSuffix)
		{
			_db = db;
		}

		public bool IgnoreErrorDocuments { get; set; }

		public int GetCount()
		{
			throw new System.NotImplementedException();
		}

		public void AddArtifactIdsIntoTempTable(ICollection<int> artifactIds)
		{
			throw new System.NotImplementedException();
		}

		public IScratchTableRepository CopyTempTable(string newTempTablePrefix)
		{
			throw new System.NotImplementedException();
		}

		public void RemoveErrorDocuments(ICollection<string> documentIDs)
		{
			throw new System.NotImplementedException();
		}

		public IDataReader GetDocumentIDsDataReaderFromTable()
		{
			throw new System.NotImplementedException();
		}

		public void DeleteTable()
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<int> ReadArtifactIDs(int offset, int size)
		{
			throw new System.NotImplementedException();
		}

		public string GetTempTableName()
		{
			throw new System.NotImplementedException();
		}

		public string GetSchemalessResourceDataBasePrepend()
		{
			throw new System.NotImplementedException();
		}

		public string GetResourceDBPrepend()
		{
			throw new System.NotImplementedException();
		}

		public void Dispose()
		{
			throw new System.NotImplementedException();
		}

	}
}