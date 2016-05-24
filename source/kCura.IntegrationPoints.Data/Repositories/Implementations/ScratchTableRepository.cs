﻿using System;
using System.Collections.Generic;
using System.Data;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class ScratchTableRepository : IScratchTableRepository
	{
		private readonly string _name;
		private readonly ITempDocTableHelper _tempHelper;
		private IDataReader _reader;
		private int _count;

		public ScratchTableRepository(string name,
			ITempDocTableHelper tempHelper,
			bool ignoreErrorDocuments)
		{
			_name = name;
			_tempHelper = tempHelper;
			IgnoreErrorDocuments = ignoreErrorDocuments;
		}

		public bool IgnoreErrorDocuments { get; }

		public int Count
		{
			get
			{
				return _count;
			}
		}

		public void AddArtifactIdsIntoTempTable(IList<int> artifactIds)
		{
			_count += artifactIds.Count;
			_tempHelper.AddArtifactIdsIntoTempTable(artifactIds, _name);
		}

		public void RemoveErrorDocument(string docIdentifier)
		{
			_count -= 1;
			_tempHelper.RemoveErrorDocument(_name, docIdentifier);
		}

		public IDataReader GetDocumentIdsDataReaderFromTable()
		{
			if (_reader == null)
			{
				_reader = _tempHelper.GetDocumentIdsDataReaderFromTable(_name);
			}
			return _reader;
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
			try
			{
				if (_reader != null)
				{
					_reader.Close();
					_reader = null;
				}
				DeleteTable();
			}
			catch (Exception)
			{
				// trying to delete temp tables early, don't have worry about failing
			}
		}
	}
}