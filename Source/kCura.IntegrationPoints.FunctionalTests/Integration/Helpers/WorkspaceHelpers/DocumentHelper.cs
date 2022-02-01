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

        public IList<DocumentTest> GetDocumentsWithImages()
        {
            return Workspace.Documents.Where(x => x.HasImages).ToList();
        }

        public IList<DocumentTest> GetDocumentsWithNatives()
        {
            return Workspace.Documents.Where(x => x.HasNatives).ToList();
        }

        public IList<DocumentTest> GetDocumentsWithNativesAndImages()
        {
            return Workspace.Documents.Where(x => x.HasNatives && x.HasImages).ToList();
        }

        public IList<DocumentTest> GetDocumentsWithImagesAndFields()
        {
            return Workspace.Documents.Where(x => x.HasFields && x.HasImages).ToList();
        }

        public IList<DocumentTest> GetDocumentsWithoutImages()
        {
            return Workspace.Documents.Where(x => !x.HasImages).ToList();
        }

        public IList<DocumentTest> GetDocumentsWithoutNatives()
        {
            return Workspace.Documents.Where(x => !x.HasNatives).ToList();
        }

        public IList<DocumentTest> GetDocumentsWithoutImagesNativesAndFields()
        {
            return Workspace.Documents.Where(x => !x.HasNatives && !x.HasImages && !x.HasFields).ToList();
        }
    }
}
