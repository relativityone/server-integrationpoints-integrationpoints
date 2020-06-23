using Relativity.Sync.WorkspaceGenerator.Settings;
using System;
using System.Data;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
	public class DataReaderProvider : IDataReaderProvider
	{
		private readonly IDocumentFactory _documentFactory;
		private readonly TestCase _testCase;
		private readonly int _batchSize = 10000;

		private int _totalRecordsProvided = 0;

		public DataReaderProvider(IDocumentFactory documentFactory, TestCase testCase, int batchSize)
		{
			_documentFactory = documentFactory;
			_testCase = testCase;
			_batchSize = batchSize;
		}

		public IDataReader GetNextDataReader()
		{
			if(_totalRecordsProvided >= _testCase.NumberOfDocuments)
			{
				return null;
			}

			int size = (_testCase.NumberOfDocuments - _totalRecordsProvided) / _batchSize == 0
				? (_testCase.NumberOfDocuments - _totalRecordsProvided) % _batchSize : _batchSize;

			Console.WriteLine($"Creating DataReader for documents ({_totalRecordsProvided} - {_totalRecordsProvided + size})...");
			IDataReader dataReader = new DataReaderWrapper(_documentFactory, _testCase, size);

			_totalRecordsProvided += size;

			return dataReader;
		}
	}
}
