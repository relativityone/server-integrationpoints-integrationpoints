namespace Relativity.Sync.WorkspaceGenerator.FileGenerating.FileContentProvider
{
    public interface IFileContentProvider
    {
        byte[] GetContent(long desiredSizeInBytes);
    }
}