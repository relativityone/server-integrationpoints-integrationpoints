namespace Relativity.Sync.Transfer
{
    internal sealed class NativeFile : FileBase, INativeFile
    {
        private static readonly INativeFile _empty = new NativeFile(0, string.Empty, string.Empty, 0);

        public NativeFile(int documentArtifactId, string location, string filename, long size)
        {
            DocumentArtifactId = documentArtifactId;
            Location = location;
            Filename = filename;
            Size = size;
        }

        public static INativeFile Empty => _empty;

        public bool IsDuplicated { get; set; } = false;
    }
}
