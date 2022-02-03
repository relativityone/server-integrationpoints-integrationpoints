using System.Collections.Generic;
using System.Linq;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public class DocumentHelper : WorkspaceHelperBase
    {
        public DocumentHelper(WorkspaceTest workspace) : base(workspace)
        {
        }

        public IList<DocumentTest> GetAllDocuments()
        {
            return Workspace.Documents;
        }

        public IList<DocumentTest> GetDocumentsWithoutImagesNativesAndFields()
        {
            return Workspace.Documents.Where(x => !x.HasNatives && !x.HasImages && !x.HasFields).ToList();
        }

        public IList<DocumentTest> GetDocumentsOnlyWithNatives()
        {
            return Workspace.Documents.Where(x => x.HasNatives && !x.HasImages && !x.HasFields).ToList();
        }
    }
}
