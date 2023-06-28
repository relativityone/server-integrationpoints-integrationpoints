namespace kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification
{
    /// <summary>
    /// Interface for file metadata writes.
    /// </summary>
    public interface IFileMetadataCollector
    {
        /// <summary>
        /// Stores file metadata using filePath as an identifier.
        /// </summary>
        /// <returns>TRUE if the metadata was stored successfully. FALSE if metadata for given file already exists.</returns>
        bool StoreMetadata(string filePath, FileMetadata metadata);
    }
}
