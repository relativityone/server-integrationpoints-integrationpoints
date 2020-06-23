using Relativity.Sync.WorkspaceGenerator.Settings;
using System;
using System.Data;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
	public class DataReaderProvider : IDataReaderProvider
	{
		private const int _MAX_READER_SIZE = 10000;

		private readonly IDocumentFactory _documentFactory;
		private readonly TestCase _testCase;

		private int _totalRecordsProvided = 0;

		public DataReaderProvider(IDocumentFactory documentFactory, TestCase testCase)
		{
			_documentFactory = documentFactory;
			_testCase = testCase;
		}

		public IDataReader GetNextDataReader()
		{
			if(_totalRecordsProvided >= _testCase.NumberOfDocuments)
			{
				return null;
			}

			int size = (_testCase.NumberOfDocuments - _totalRecordsProvided) / _MAX_READER_SIZE == 0
				? (_testCase.NumberOfDocuments - _totalRecordsProvided) % _MAX_READER_SIZE : _MAX_READER_SIZE;

			Console.WriteLine($"Creating DataReader for documents ({_totalRecordsProvided} - {_totalRecordsProvided + size})...");
			IDataReader dataReader = new DataReaderWrapper(_documentFactory, _testCase, size);

			_totalRecordsProvided += size;

			return dataReader;
		}
	}
}
