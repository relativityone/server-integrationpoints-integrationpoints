

using System;
using System.Collections.Generic;
using SystemInterface.IO;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
	public class FolderTreeProvider<TTreeItem> : DirectoryTreeCreator<TTreeItem>, IFolderTreeProvider<TTreeItem> where TTreeItem : JsTreeItemDTO, new()
	{
		public override List<TTreeItem> GetChildren(string path, bool isRoot, bool includeFiles = false)
		{
			throw new NotImplementedException();
		}

		public FolderTreeProvider(IDirectory directory, IHelper helper) : base(directory, helper)
		{
		}
	}
}
