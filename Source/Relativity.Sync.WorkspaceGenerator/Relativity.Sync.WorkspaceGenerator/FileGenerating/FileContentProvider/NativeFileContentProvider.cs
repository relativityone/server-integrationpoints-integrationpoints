namespace Relativity.Sync.WorkspaceGenerator.FileGenerating.FileContentProvider
{
    public class NativeFileContentProvider : IFileContentProvider
    {
        public byte[] GetContent(long desiredSizeInBytes)
        {
            return new byte[desiredSizeInBytes];
        }
    }
}