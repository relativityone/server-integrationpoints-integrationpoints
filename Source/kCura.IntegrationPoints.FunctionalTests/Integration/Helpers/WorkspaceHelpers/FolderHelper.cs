using System.Collections.Generic;
using System.Linq;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public class FolderHelper : WorkspaceHelperBase
    {
        private DocumentHelper _documentHelper;

        public FolderHelper(WorkspaceTest workspace) : base(workspace)
        {
            _documentHelper = Workspace.Helpers.DocumentHelper;
        }

        public IList<FolderTest> GetAllFolders()
        {
            return Workspace.Folders;
        }

        public FolderTest GetFolder(int folderId)
        {
            FolderTest folder = Workspace.Folders.First(x => x.ArtifactId == folderId);
            return folder;
        }
    }
}
