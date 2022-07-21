namespace Relativity.Sync.Transfer
{
    internal sealed class NativeFile : INativeFile
    {
        public static readonly INativeFile _empty = new NativeFile(0, string.Empty, string.Empty, 0);

        public NativeFile(int documentArtifactId, string location, string filename, long size)
        {
            DocumentArtifactId = documentArtifactId;
            Location = location;
            Filename = filename;
            Size = size;
        }

        public int DocumentArtifactId { get; }
        public string Location { get; }
        public string Filename { get; }
        public long Size { get; }
        public bool IsDuplicated { get; set; } = false;

        public static INativeFile Empty => _empty;
    }
}
