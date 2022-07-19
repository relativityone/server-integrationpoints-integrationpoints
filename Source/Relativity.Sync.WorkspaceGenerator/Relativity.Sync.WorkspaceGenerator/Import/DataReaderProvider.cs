using Relativity.Sync.WorkspaceGenerator.Settings;
using System;
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

        private int _totalRecordsProvided = 0;
        
        public DataReaderProvider(IDocumentFactory documentFactory, IImageGenerator imageGenerator, TestCase testCase,
            int documentsToImportCount, int batchSize)
        {
            _documentFactory = documentFactory;
            _imageGenerator = imageGenerator;
            _testCase = testCase;
            _documentsToImportCount = documentsToImportCount;
            _batchSize = batchSize;
        }

        public IDataReaderWrapper GetNextDocumentDataReader()
        {
            int documentCount = CalculateReaderDocumentCount();

            return GetDataReaderWrapper(documentCount, () =>
                 new DocumentDataReaderWrapper(_documentFactory, _testCase, documentCount, _totalRecordsProvided));
        }

        public IDataReaderWrapper GetNextImageDataReader()
        {
            int documentCount = CalculateReaderDocumentCount();

            return GetDataReaderWrapper(documentCount, () =>
                 new ImageDataReaderWrapper(_imageGenerator, _documentFactory, _testCase, documentCount, _totalRecordsProvided));
        }

        private IDataReaderWrapper GetDataReaderWrapper<T>(int documentCount, Func<T> factoryMethod) where T : IDataReaderWrapper
        {
            if (_totalRecordsProvided >= _documentsToImportCount)
            {
                return null;
            }

            Console.WriteLine(
                $"Creating {typeof(T).Name} ({_totalRecordsProvided} - {_totalRecordsProvided + documentCount})...");

            IDataReaderWrapper dataReader = factoryMethod();

            _totalRecordsProvided += documentCount;

            return dataReader;
        }

        private int CalculateReaderDocumentCount()
        {
            return (_documentsToImportCount - _totalRecordsProvided) / _batchSize == 0
                ? (_documentsToImportCount - _totalRecordsProvided) % _batchSize : _batchSize;
        }
    }
}
