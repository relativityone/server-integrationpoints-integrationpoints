using System.Collections.Concurrent;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification
{
    /// <summary>
    /// The file metadata in-memory store.
    /// </summary>
    public class FileMetadataStore : IFileMetadataCollector, IReadOnlyFileMetadataStore
    {
        private ConcurrentDictionary<string, FileMetadata> _fileMetadataDictionary = new ConcurrentDictionary<string, FileMetadata>();

        /// <inheritdoc />
        public bool StoreMetadata(string filePath, FileMetadata metadata)
        {
            return _fileMetadataDictionary.TryAdd(filePath, metadata);
        }

        /// <inheritdoc />
        public FileMetadata GetMetadata(string filePath)
        {
            if (_fileMetadataDictionary.TryGetValue(filePath, out FileMetadata result) == false)
            {
                throw new KeyNotFoundException();
            }

            return result;
        }
    }
}
