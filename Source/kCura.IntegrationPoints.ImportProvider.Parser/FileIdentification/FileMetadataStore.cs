using System.Collections.Concurrent;
using Relativity.API;

namespace kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification
{
    /// <summary>
    /// The file metadata in-memory store.
    /// </summary>
    public class FileMetadataStore : IFileMetadataCollector, IReadOnlyFileMetadataStore
    {
        private ConcurrentDictionary<string, FileMetadata> _fileMetadataDictionary = new ConcurrentDictionary<string, FileMetadata>();
        private readonly IAPILog _logger;

        public FileMetadataStore(IAPILog logger)
        {
            _logger = logger;
            _logger.LogInformation("FileMetadataStore creation.");
        }

        /// <inheritdoc />
        public bool StoreMetadata(string filePath, FileMetadata metadata)
        {
            _logger.LogInformation("Adding {filePath} with {@metadata}", filePath, metadata);
            return _fileMetadataDictionary.TryAdd(filePath, metadata);
        }

        /// <inheritdoc />
        public FileMetadata GetMetadata(string filePath)
        {
            FileMetadata fileMetadata = _fileMetadataDictionary.TryGetValue(filePath, out FileMetadata result)
                ? result
                : null;

            _logger.LogInformation("For {filePath} Metadata has been read - {@metadata}", filePath, fileMetadata);

            return fileMetadata;
        }
    }
}
