namespace kCura.IntegrationPoints.ImportProvider.FileIdentification
{
    public class FileProperties
    {
        public int Id { get; }

        public string Description { get; }

        public long Size { get; set; }

        public FileProperties(int id, string description, long size)
        {
            Id = id;
            Description = description;
            Size = size;
        }
    }
}
