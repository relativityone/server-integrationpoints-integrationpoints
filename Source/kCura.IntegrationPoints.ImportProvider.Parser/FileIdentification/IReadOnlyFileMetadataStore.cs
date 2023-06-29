using System.Collections.Generic;

namespace kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification
{
    /// <summary>
    /// Interface for file metadata reads.
    /// </summary>
    public interface IReadOnlyFileMetadataStore
    {
        /// <summary>
        /// Gets the metadata for given file.
        /// </summary>
        /// <exception cref="KeyNotFoundException"></exception>
        FileMetadata GetMetadata(string filePath);
    }
}
