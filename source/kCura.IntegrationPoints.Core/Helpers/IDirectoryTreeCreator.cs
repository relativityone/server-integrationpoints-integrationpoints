using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers
{
	public interface IDirectoryTreeCreator
	{
		JsTreeItemDTO TraverseTree(string root);
	}
}