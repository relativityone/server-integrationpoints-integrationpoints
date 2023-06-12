using System.Diagnostics;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Interfaces
{
    public interface IReadOnlyFileMetadataStore
    {
        FileProperties GetMetadata(string filePath);

        bool IsPopulated { get; }
    }
}
