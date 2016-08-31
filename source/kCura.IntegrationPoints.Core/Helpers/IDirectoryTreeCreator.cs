using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Helpers
{
	public interface IDirectoryTreeCreator
	{
		DirectoryTreeItem TraverseTree(string root);
	}
}
