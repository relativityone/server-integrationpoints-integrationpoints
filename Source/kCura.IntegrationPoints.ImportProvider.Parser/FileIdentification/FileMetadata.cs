namespace kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification
{
    /// <summary>
    /// Represents file metadata retrieved from Outside-In
    /// </summary>
    public class FileMetadata
    {
        public FileMetadata(int typeId, string description, long size)
        {
            TypeId = typeId;
            Description = description;
            Size = size;
        }

        public int TypeId { get; }

        public string Description { get; }

        public long Size { get; }
    }
}
