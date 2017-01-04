using System;
using System.Collections.Generic;
using System.IO;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
	public class RelativePathDirectoryTreeCreator<TTreeItem> : IRelativePathDirectoryTreeCreator<TTreeItem> where TTreeItem : JsTreeItemDTO, new()
	{
		private readonly IDirectoryTreeCreator<TTreeItem> _directoryTreeCreator;
		private readonly IDataTransferLocationService _dataTransferLocationService;

		public RelativePathDirectoryTreeCreator(IDirectoryTreeCreator<TTreeItem> directoryTreeCreator, IDataTransferLocationService dataTransferLocationService)
		{
			_directoryTreeCreator = directoryTreeCreator;
			_dataTransferLocationService = dataTransferLocationService;
		}

		public List<TTreeItem> GetChildren(string relativePath, bool isRoot, int workspaceId, Guid integrationPointTypeIdentifier,
			bool includeFiles = false)
		{
			string rootPath = _dataTransferLocationService.GetRootLocationFor(workspaceId);
			string providerTypePath = _dataTransferLocationService.GetDefaultRelativeLocationFor(integrationPointTypeIdentifier);

			string path = Path.Combine(rootPath, String.IsNullOrWhiteSpace(relativePath) ? providerTypePath : relativePath);

			List<TTreeItem> treeItems = _directoryTreeCreator.GetChildren(path, isRoot, includeFiles);
			// we need to remove workspace file share path prefix
			foreach (TTreeItem treeItem in treeItems)
			{
				treeItem.Text = treeItem.Text.Replace(rootPath, string.Empty);
				treeItem.Id = treeItem.Id.Replace(rootPath, string.Empty);
			}
			return treeItems;
		}

	}
}
