namespace Relativity.Sync.WorkspaceGenerator.FileGenerating.FileContentProvider
{
    public class AsciiExtractedTextFileContentProvider : IFileContentProvider
    {
        public byte[] GetContent(long desiredSizeInBytes)
        {
            const byte character = 46; // . (dot) character
            byte[] data = new byte[desiredSizeInBytes];
            for (long i = 0; i < desiredSizeInBytes; i++)
            {
                data[i] = character;
            }

            return data;
        }
    }
}