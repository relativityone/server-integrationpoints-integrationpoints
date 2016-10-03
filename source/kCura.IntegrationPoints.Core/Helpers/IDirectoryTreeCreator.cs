using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers
{
	public interface IDirectoryTreeCreator<T> where T : JsTreeItemBaseDTO
	{
		T TraverseTree(string root);
		List<T> GetChildren(string path, bool isRoot);
	}
}