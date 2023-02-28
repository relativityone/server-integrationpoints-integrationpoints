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
        #region Fields

        private readonly IDirectoryTreeCreator<TTreeItem> _directoryTreeCreator;
        private readonly IDataTransferLocationService _dataTransferLocationService;
        public const string FILESHARE_PLACEHOLDER_PREFIX = @".\";

        #endregion // Fields

        #region Constructors

        public RelativePathDirectoryTreeCreator(IDirectoryTreeCreator<TTreeItem> directoryTreeCreator, IDataTransferLocationService dataTransferLocationService)
        {
            _directoryTreeCreator = directoryTreeCreator;
            _dataTransferLocationService = dataTransferLocationService;
        }

        #endregion // Constructors

        #region Methods

        public List<TTreeItem> GetChildren(string relativePath, bool isRoot, int workspaceId, Guid integrationPointTypeIdentifier,
            bool includeFiles = false)
        {
            string rootPath = _dataTransferLocationService.GetWorkspaceFileLocationRootPath(workspaceId);
            string defaultRelativePathForProviderType = _dataTransferLocationService.GetDefaultRelativeLocationFor(integrationPointTypeIdentifier);

            string path = Path.Combine(rootPath, string.IsNullOrWhiteSpace(relativePath) ? defaultRelativePathForProviderType : relativePath);

            List<TTreeItem> treeItems = _directoryTreeCreator.GetChildren(path, isRoot, includeFiles);
            // we need to remove workspace fileshare uri path part and leave only relative path
            RemoveWorkspaceFileShareUrlPart(rootPath, treeItems);
            if (isRoot)
            {
                TTreeItem rootTreeItem = treeItems.First();
                // for root we need to append (relative) prefix to show on UI
                rootTreeItem.Text = AppendFileSharePlaceholderPathPrefix(rootTreeItem.Text, rootPath, workspaceId);
                RemoveWorkspaceFileShareUrlPart(rootPath, rootTreeItem.Children.Cast<TTreeItem>().ToList());
            }
            return treeItems;
        }

        private string AppendFileSharePlaceholderPathPrefix(string path, string rootPath, int workspaceId)
        {
            return Path.Combine(FILESHARE_PLACEHOLDER_PREFIX, $"EDDS{workspaceId}", GetLocationTruncatedByRootPath(path, rootPath));
        }

        private void RemoveWorkspaceFileShareUrlPart(string rootPath, List<TTreeItem> treeItems)
        {
            treeItems.ForEach(treeItem => treeItem.Id = GetLocationTruncatedByRootPath(treeItem.Id, rootPath));
        }

        private string GetLocationTruncatedByRootPath(string url, string rootPath)
        {
            return url.Replace(rootPath, string.Empty).TrimStart('\\');
        }

        #endregion // Methods
    }
}
