namespace Relativity.Sync.WorkspaceGenerator.FileGenerator.FileContentProvider
{
	public class NativeFileContentProvider : IFileContentProvider
	{
		public byte[] GetContent(long desiredSizeInBytes)
		{
			return new byte[desiredSizeInBytes];
		}
	}
}