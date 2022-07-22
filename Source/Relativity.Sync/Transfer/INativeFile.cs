namespace Relativity.Sync.Transfer
{
    internal interface INativeFile
    {
        int DocumentArtifactId { get; }
        string Location { get; }
        string Filename { get; }
        long Size { get; }
        bool IsDuplicated { get; set; }
    }
}
