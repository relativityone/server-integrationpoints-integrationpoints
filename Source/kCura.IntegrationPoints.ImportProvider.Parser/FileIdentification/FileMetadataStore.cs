using System.Collections.Concurrent;
using kCura.IntegrationPoints.Domain.Logging;

namespace kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification
{
    /// <summary>
    /// The file metadata in-memory store.
    /// </summary>
    public class FileMetadataStore : IFileMetadataCollector, IReadOnlyFileMetadataStore
    {
        private readonly IDiagnosticLog _logger;
        private readonly ConcurrentDictionary<string, FileMetadata> _fileMetadataDictionary = new ConcurrentDictionary<string, FileMetadata>();

        private bool _isCompleted;

        public FileMetadataStore(IDiagnosticLog logger)
        {
            _logger = logger;
            _logger.LogDiagnostic("FileMetadataStore creation.");
        }

        /// <inheritdoc />
        public bool StoreMetadata(string filePath, FileMetadata metadata)
        {
            _logger.LogDiagnostic("Adding {filePath} with {@metadata}", filePath, metadata);
            return _fileMetadataDictionary.TryAdd(filePath, metadata);
        }

        /// <inheritdoc />
        public FileMetadata GetMetadata(string filePath)
        {
            FileMetadata fileMetadata = _fileMetadataDictionary.TryGetValue(filePath, out FileMetadata result)
                ? result
                : null;

            _logger.LogDiagnostic("For {filePath} Metadata has been read - {@metadata}", filePath, fileMetadata);

            return fileMetadata;
        }
    }
}
