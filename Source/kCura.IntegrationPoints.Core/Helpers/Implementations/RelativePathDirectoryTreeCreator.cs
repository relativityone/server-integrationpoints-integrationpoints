using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
	public class RelativePathDirectoryTreeCreator<TTreeItem> : IRelativePathDirectoryTreeCreator<TTreeItem> where TTreeItem : JsTreeItemDTO, new()
	{
		private readonly IDirectoryTreeCreator<TTreeItem> _directoryTreeCreator;
		private readonly IDataTransferLocationService _dataTransferLocationService;

		public const string FILESHARE_PLACEHOLDER_PREFIX = @".\";

		public RelativePathDirectoryTreeCreator(IDirectoryTreeCreator<TTreeItem> directoryTreeCreator, IDataTransferLocationService dataTransferLocationService)
		{
			_directoryTreeCreator = directoryTreeCreator;
			_dataTransferLocationService = dataTransferLocationService;
		}

		public List<TTreeItem> GetChildren(string relativePath, bool isRoot, int workspaceId, Guid integrationPointTypeIdentifier,
			bool includeFiles = false)
		{
			string rootPath = _dataTransferLocationService.GetWorkspaceFileLocationRootPath(workspaceId);
			string providerTypePath = _dataTransferLocationService.GetDefaultRelativeLocationFor(integrationPointTypeIdentifier);

			string path = Path.Combine(rootPath, String.IsNullOrWhiteSpace(relativePath) ? providerTypePath : relativePath);

			List<TTreeItem> treeItems = _directoryTreeCreator.GetChildren(path, isRoot, includeFiles);
			// we need to remove workspace file share path prefix
			RemoveWorkspaceFileShareUrlPart(rootPath, treeItems);
			if (isRoot)
			{
				TTreeItem rootTreeItem = treeItems.First();
				rootTreeItem.Text = AppendFileSharePlaceholderPathPrefix(rootTreeItem.Text, rootPath);
				RemoveWorkspaceFileShareUrlPart(rootPath, rootTreeItem.Children.Cast<TTreeItem>().ToList());
			}
			return treeItems;
		}

		private string AppendFileSharePlaceholderPathPrefix(string path, string rootPath)
		{
			return Path.Combine(FILESHARE_PLACEHOLDER_PREFIX, GetLocationTruncatedByRootPath(path, rootPath));
		}

		private void RemoveWorkspaceFileShareUrlPart(string rootPath, List<TTreeItem> treeItems)
		{
			foreach (TTreeItem treeItem in treeItems)
			{
				treeItem.Id = GetLocationTruncatedByRootPath(treeItem.Id, rootPath);
			}
		}

		private string GetLocationTruncatedByRootPath(string url, string rootPath)
		{
			string trancatedUrl = url.Replace(rootPath, string.Empty);
			return trancatedUrl.TrimStart('\\');			
		}
	}
}
