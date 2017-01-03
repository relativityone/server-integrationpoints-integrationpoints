using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers
{
	/// <summary>
	/// This interface will replace IDirectoryTreeCreator when Import Load File provider will be using Workspace Fileshare as Destination Folder
	/// </summary>
	public interface IFolderTreeProvider<T> where T : JsTreeItemBaseDTO
	{
		List<T> GetChildren(string path, bool isRoot, bool includeFiles = false);
	}
}
