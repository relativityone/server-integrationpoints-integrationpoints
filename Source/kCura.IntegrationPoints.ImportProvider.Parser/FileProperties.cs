namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class FileProperties
    {
        public int TypeId { get; }

        public string Description { get; }

        public long Size { get; }

        public FileProperties(int typeId, string description, long size)
        {
            TypeId = typeId;
            Description = description;
            Size = size;
        }
    }
}
