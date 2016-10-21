using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers
{
	public interface IDirectoryTreeCreator<T> where T : JsTreeItemBaseDTO
	{
		T TraverseTree(string root, bool includeFiles = false);
		List<T> GetChildren(string path, bool isRoot);
	}
}