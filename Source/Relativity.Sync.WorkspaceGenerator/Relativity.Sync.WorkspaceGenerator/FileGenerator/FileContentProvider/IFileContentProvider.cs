namespace Relativity.Sync.WorkspaceGenerator.FileGenerator.FileContentProvider
{
	public interface IFileContentProvider
	{
		byte[] GetContent(long desiredSizeInBytes);
	}
}