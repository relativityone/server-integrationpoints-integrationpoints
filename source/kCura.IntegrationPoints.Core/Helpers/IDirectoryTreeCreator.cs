using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers
{
	public interface IDirectoryTreeCreator<T> : IFolderTreeProvider<T> where T : JsTreeItemBaseDTO
	{
		T TraverseTree(string root, bool includeFiles = false);
	}
}