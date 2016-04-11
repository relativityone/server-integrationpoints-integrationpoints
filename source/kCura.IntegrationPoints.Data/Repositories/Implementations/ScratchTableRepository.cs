using System;
using System.Collections.Generic;
using System.Data;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class ScratchTableRepository : IScratchTableRepository
	{
		private readonly string _name;
		private readonly ITempDocTableHelper _tempHelper;
		private IDataReader _reader;
		public ScratchTableRepository(string name,
			ITempDocTableHelper tempHelper)
		{
			_name = name;
			_tempHelper = tempHelper;
		}

		public void AddArtifactIdsIntoTempTable(List<int> artifactIds)
		{
			_tempHelper.AddArtifactIdsIntoTempTable(artifactIds, _name);
		}

		public void RemoveErrorDocument(string docIdentifier)
		{
			_tempHelper.RemoveErrorDocument(docIdentifier);
		}

		public IDataReader GetDocumentIdsDataReaderFromTable()
		{
			if (_reader == null)
			{
				_reader = _tempHelper.GetDocumentIdsDataReaderFromTable(_name);
			}
			return _reader;
		}

		public List<int> GetDocumentIdsFromTable()
		{
			return _tempHelper.GetDocumentIdsFromTable(_name);
		}

		public void DeleteTable()
		{
			_tempHelper.DeleteTable(_name);
		}

		public string GetTempTableName()
		{
			return _tempHelper.GetTempTableName(_name);
		}

		public void Dispose()
		{
			if (_reader != null)
			{
				_reader.Close();
				_reader = null;
			}
			DeleteTable();
		}
	}
}