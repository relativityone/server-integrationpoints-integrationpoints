using Relativity.Sync.WorkspaceGenerator.Settings;
using System;
using System.Data;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
	public class DataReaderProvider : IDataReaderProvider
	{
		private readonly IDocumentFactory _documentFactory;
		private readonly TestCase _testCase;
		private readonly int _documentsToImportCount;
		private readonly int _batchSize;
		
		private int _totalRecordsProvided = 0;

		public DataReaderProvider(IDocumentFactory documentFactory, TestCase testCase, int documentsToImportCount, int batchSize)
		{
			_documentFactory = documentFactory;
			_testCase = testCase;
			_documentsToImportCount = documentsToImportCount;
			_batchSize = batchSize;
		}

		public IDataReader GetNextDataReader()
		{
			if(_totalRecordsProvided >= _documentsToImportCount)
			{
				return null;
			}

			int size = (_documentsToImportCount - _totalRecordsProvided) / _batchSize == 0
				? (_documentsToImportCount - _totalRecordsProvided) % _batchSize : _batchSize;

			Console.WriteLine($"Creating DataReader for documents ({_totalRecordsProvided} - {_totalRecordsProvided + size})...");
			IDataReader dataReader = new DataReaderWrapper(_documentFactory, _testCase, size);

			_totalRecordsProvided += size;

			return dataReader;
		}
	}
}
