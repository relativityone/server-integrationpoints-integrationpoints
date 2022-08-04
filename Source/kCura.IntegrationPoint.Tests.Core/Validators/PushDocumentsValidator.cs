namespace kCura.IntegrationPoint.Tests.Core.Validators
{
    public class PushDocumentsValidator : DocumentsValidator
    {
        public PushDocumentsValidator(int sourceWorkspaceId, int destinationWorkspaceId, params IDocumentValidator[] documentValidators) : 
            base(() => DocumentService.GetAllDocuments(sourceWorkspaceId), () => DocumentService.GetAllDocuments(destinationWorkspaceId), documentValidators)
        {
        }
    }
}