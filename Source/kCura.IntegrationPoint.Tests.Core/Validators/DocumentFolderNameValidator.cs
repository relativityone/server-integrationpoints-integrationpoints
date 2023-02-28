namespace kCura.IntegrationPoint.Tests.Core.Validators
{
    public class DocumentFolderNameValidator : DocumentPropertyValidator<string>
    {
        public DocumentFolderNameValidator() : base(document => document.FolderName)
        {
        }
    }
}
