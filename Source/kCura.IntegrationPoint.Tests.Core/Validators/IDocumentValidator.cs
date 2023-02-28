using kCura.IntegrationPoint.Tests.Core.Models;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoint.Tests.Core.Validators
{
    public interface IDocumentValidator
    {
        void ValidateDocument(Document destinationDocument, Document sourceDocument);
    }
}
