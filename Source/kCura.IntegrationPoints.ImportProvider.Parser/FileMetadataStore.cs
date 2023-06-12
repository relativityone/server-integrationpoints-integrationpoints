using System.Collections.Concurrent;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class FileMetadataStore : IReadOnlyFileMetadataStore
    {
        private ConcurrentDictionary<string, FileProperties> _fileMetadataDictionary = new ConcurrentDictionary<string, FileProperties>();

        public bool IsPopulated => _fileMetadataDictionary.Count > 0;

        public FileProperties GetMetadata(string filePath)
        {
            return new FileProperties(1027, "Microsoft Excel 2000", 20);
        }
    }
}
