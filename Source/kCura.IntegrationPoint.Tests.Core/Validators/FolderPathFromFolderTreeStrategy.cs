using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Folder;

namespace kCura.IntegrationPoint.Tests.Core.Validators
{
    public class FolderPathFromFolderTreeStrategy : FolderPathStrategyWithCache
    {
        private readonly IFolderManager _folderManager;
        private readonly bool _includeWorkspaceFolderInPath;
        private readonly int _workspaceId;

        public FolderPathFromFolderTreeStrategy(int workspaceId, IFolderManager folderManager, bool includeWorkspaceFolderInPath = false)
        {
            _folderManager = folderManager;
            _includeWorkspaceFolderInPath = includeWorkspaceFolderInPath;
            _workspaceId = workspaceId;
        }

        protected override string GetFolderPathInternal(Document document)
        {
            string folderPath = GetFullFolderPathFromManager(document);

            if (!_includeWorkspaceFolderInPath)
            {
                folderPath = StripWorkspaceNameFromFolderPath(folderPath);
            }
            
            return folderPath;
        }

        private string GetFullFolderPathFromManager(Document document)
        {
            Task<List<FolderPath>> folderLookup = _folderManager.GetFullPathListAsync(_workspaceId, new List<int>() { document.ParentArtifactId });
            folderLookup.Wait();
            if (folderLookup.Result == null || folderLookup.Result.Count == 0)
            {
                throw new Exception($"Cannot find folder for document. Workspace id: {_workspaceId}. Document control number: {document.ControlNumber}. Document folder: {document.FolderName}");
            }

            string result = folderLookup.Result.First().FullPath;
            return result.Replace(@" \ ", FOLDER_TREE_SEPARATOR);
        }

        private string StripWorkspaceNameFromFolderPath(string folderPath)
        {
            int firstSparator = folderPath.IndexOf(FOLDER_TREE_SEPARATOR, StringComparison.OrdinalIgnoreCase);
            if (firstSparator == -1)
            {
                return string.Empty;
            }

            return folderPath.Substring(firstSparator + FOLDER_TREE_SEPARATOR.Length);
        }
    }
}