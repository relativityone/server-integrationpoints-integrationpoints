namespace Relativity.Sync.Transfer
{
    internal interface IFile
    {
        int DocumentArtifactId { get; }

        string Location { get; }

        string Filename { get; }

        long Size { get; }

        bool IsMalwareDetected { get; }
    }
}
