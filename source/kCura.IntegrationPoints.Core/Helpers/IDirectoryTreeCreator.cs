using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers
{
	public interface IDirectoryTreeCreator
	{
		TreeItemDTO TraverseTree(string root, bool includeFiles = false);
	}
}