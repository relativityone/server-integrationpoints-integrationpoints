namespace Relativity.Sync.Transfer
{
    internal class ImageFile : FileBase, IFile
    {
        public ImageFile(int documentArtifactId, string identifier, string location, string filename, long size, int? productionId = null)
        {
            DocumentArtifactId = documentArtifactId;
            Location = location;
            Filename = filename;
            Size = size;
            ProductionId = productionId;
            Identifier = identifier;
        }

        public int? ProductionId { get; }

        public string Identifier { get; }
    }
}
