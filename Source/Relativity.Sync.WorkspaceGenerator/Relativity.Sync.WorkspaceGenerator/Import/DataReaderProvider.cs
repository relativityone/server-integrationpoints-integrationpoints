using Relativity.Sync.WorkspaceGenerator.Settings;
using System;
using System.Data;
using Relativity.Sync.WorkspaceGenerator.FileGenerating;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
	public class DataReaderProvider : IDataReaderProvider
	{
		private readonly IDocumentFactory _documentFactory;
		private readonly IImageGenerator _imageGenerator;
		private readonly TestCase _testCase;
		private readonly int _documentsToImportCount;
		private readonly int _batchSize;
		private readonly int _imageBatchSize;

		private int _totalRecordsProvided = 0;

		public DataReaderProvider(IDocumentFactory documentFactory, IImageGenerator imageGenerator, TestCase testCase,
			int documentsToImportCount, int batchSize)
		{
			_documentFactory = documentFactory;
			_imageGenerator = imageGenerator;
			_testCase = testCase;
			_documentsToImportCount = documentsToImportCount;
			_batchSize = batchSize;
			_imageBatchSize = batchSize / imageGenerator.SetPerDocumentCount;
		}

		public IDataReaderWrapper GetNextDocumentDataReader()
		{
			if (_totalRecordsProvided >= _documentsToImportCount)
			{
				return null;
			}

			int size = (_documentsToImportCount - _totalRecordsProvided) / _batchSize == 0
				? (_documentsToImportCount - _totalRecordsProvided) % _batchSize : _batchSize;

			Console.WriteLine($"Creating DataReader for documents ({_totalRecordsProvided} - {_totalRecordsProvided + size})...");
			IDataReaderWrapper dataReader = new DocumentDataReaderWrapper(_documentFactory, _testCase, size, _totalRecordsProvided);

			_totalRecordsProvided += size;

			return dataReader;
		}

		public IDataReaderWrapper GetNextImageDataReader()
		{
			if (_totalRecordsProvided >= _documentsToImportCount)
			{
				return null;
			}

			int size = (_documentsToImportCount - _totalRecordsProvided) / _imageBatchSize == 0
				? (_documentsToImportCount - _totalRecordsProvided) % _imageBatchSize : _imageBatchSize;

			Console.WriteLine($"Creating DataReader for document images ({_totalRecordsProvided} - {_totalRecordsProvided + size})...");
			IDataReaderWrapper dataReader = new ImageDataReaderWrapper(_imageGenerator, _documentFactory, _testCase, size, _totalRecordsProvided);

			_totalRecordsProvided += size;

			return dataReader;
		}
	}
}
