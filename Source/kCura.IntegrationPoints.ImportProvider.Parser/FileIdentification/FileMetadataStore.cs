using System.Collections.Concurrent;
using kCura.IntegrationPoints.Common;

namespace kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification
{
    /// <summary>
    /// The file metadata in-memory store.
    /// </summary>
    public class FileMetadataStore : IFileMetadataCollector, IReadOnlyFileMetadataStore
    {
        private readonly ILogger<FileMetadataStore> _logger;
        private readonly ConcurrentDictionary<string, FileMetadata> _fileMetadataDictionary = new ConcurrentDictionary<string, FileMetadata>();

        public FileMetadataStore(ILogger<FileMetadataStore> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public bool StoreMetadata(string filePath, FileMetadata metadata)
        {
            return _fileMetadataDictionary.TryAdd(filePath, metadata);
        }

        /// <inheritdoc />
        public FileMetadata GetMetadata(string filePath)
        {
            if (_fileMetadataDictionary.TryGetValue(filePath, out FileMetadata fileMetadata) == false)
            {
                _logger.LogWarning("Unable to get file metadata for file {filePath}", filePath);
            }

            return fileMetadata;
        }
    }
}
