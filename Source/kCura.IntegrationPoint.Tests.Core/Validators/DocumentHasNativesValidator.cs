using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.Validators
{
    public class DocumentHasNativesValidator : IDocumentValidator
    {
        protected bool? ExpectHasNatives { get; }

        public DocumentHasNativesValidator(bool? expectHasNatives)
        {
            ExpectHasNatives = expectHasNatives;
        }

        public virtual void ValidateDocument(Document destinationDocument, Document sourceDocument)
        {
            bool shouldExpectNatives = ShouldExpectNativesForDocument(sourceDocument);

            Assert.That(destinationDocument.HasNatives, Is.EqualTo(shouldExpectNatives));
        }

        protected  virtual bool ShouldExpectNativesForDocument(Document expectedDocument)
        {
            return ExpectHasNatives ?? expectedDocument.HasNatives.GetValueOrDefault();
        }
    }
}
