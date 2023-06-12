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
            bool result = _fileMetadataDictionary.TryAdd(filePath, metadata);
            IsPopulated = true;
            return result;
        }

        /// <inheritdoc />
        public FileMetadata GetMetadata(string filePath)
        {
            FileMetadata result;
            if (_fileMetadataDictionary.TryGetValue(filePath, out result) == false)
            {
                throw new KeyNotFoundException();
            }

            return result;
        }

        /// <inheritdoc />
        public bool IsPopulated { get; private set; }
    }
}
