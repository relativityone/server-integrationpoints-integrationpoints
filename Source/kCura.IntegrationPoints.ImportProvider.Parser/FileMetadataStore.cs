using System.Collections.Concurrent;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class FileMetadataStore : IReadOnlyFileMetadataStore
    {
        private ConcurrentDictionary<string, FileProperties> _fileMetadataDictionary = new ConcurrentDictionary<string, FileProperties>();

        public FileProperties GetMetadata(string filePath)
        {
            FileProperties result;
            if (_fileMetadataDictionary.TryGetValue(filePath, out result) == false)
            {
                // TODO throw??
            }

            return result;
        }
    }
}
