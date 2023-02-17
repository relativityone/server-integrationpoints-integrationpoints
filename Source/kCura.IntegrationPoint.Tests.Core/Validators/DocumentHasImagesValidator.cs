using System;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.Validators
{
    public class DocumentHasImagesValidator : IDocumentValidator
    {
        protected bool? ExpectHasImages { get; }

        public DocumentHasImagesValidator(bool? expectHasImages)
        {
            ExpectHasImages = expectHasImages;
        }

        public virtual void ValidateDocument(Document destinationDocument, Document sourceDocument)
        {
            bool? destinationDocumentHasImages = DestinationDocumentHasImagesValue(destinationDocument);

            Assert.That(destinationDocumentHasImages, Is.EqualTo(ExpectHasImages), $"Invalid document '{destinationDocument[IntegrationPoints.Data.DocumentFields.ControlNumber]}' HasImage field value");
        }

        protected virtual bool? DestinationDocumentHasImagesValue(Document destinationDocument)
        {
            string hasImagesChoiceValue = destinationDocument.HasImages.Name;

            if (string.IsNullOrEmpty(hasImagesChoiceValue))
            {
                return null;
            }

            return string.Equals(hasImagesChoiceValue, "Yes", StringComparison.OrdinalIgnoreCase) || string.Equals(hasImagesChoiceValue, "True", StringComparison.OrdinalIgnoreCase);
        }
    }
}
