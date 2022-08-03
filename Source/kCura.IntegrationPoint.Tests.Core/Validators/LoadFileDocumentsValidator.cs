using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoint.Tests.Core.Validators
{
    public class LoadFileDocumentsValidator : DocumentsValidator
    {
        public LoadFileDocumentsValidator(IEnumerable<Document> expectedDocuments, int destinationWorkspaceId, params IDocumentValidator[] documentValidators)
            : base(expectedDocuments.ToList, () => DocumentService.GetAllDocuments(destinationWorkspaceId), documentValidators)
        {
        }
    }
}