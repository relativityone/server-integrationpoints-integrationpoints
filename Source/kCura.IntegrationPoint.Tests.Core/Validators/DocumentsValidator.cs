using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.Validators
{
    public class DocumentsValidator : IValidator
    {
        private readonly Func<IList<Document>> _expectedDocumentsProvider;
        private readonly Func<IList<Document>> _actualDocumentsProvider;
        public List<IDocumentValidator> DocumentValidators { get; }


        public DocumentsValidator(Func<IList<Document>> expectedDocumentsProvider, Func<IList<Document>> actualDocumentsProvider, params IDocumentValidator[] documentValidators)
        {
            _expectedDocumentsProvider = expectedDocumentsProvider;
            _actualDocumentsProvider = actualDocumentsProvider;

            DocumentValidators = new List<IDocumentValidator>(documentValidators);
        }

        public virtual void Validate()
        {
            IList<Document> expectedDocuments = _expectedDocumentsProvider();
            IList<Document> actualDocuments = _actualDocumentsProvider();

            ValidateDocuments(expectedDocuments, actualDocuments);
        }

        public virtual DocumentsValidator ValidateWith(IDocumentValidator documentValidator)
        {
            DocumentValidators.Add(documentValidator);

            return this;
        }

        protected virtual void ValidateDocuments(IList<Document> expectedDocuments, IList<Document> actualDocuments)
        {
            int actualDocumentsCount = actualDocuments.Count;
            int expectedDocumentsCount = expectedDocuments.Count;

            Assert.That(actualDocumentsCount, Is.EqualTo(expectedDocumentsCount), "Number of documents is different. Actual: {0} Expected: {1}.", actualDocumentsCount, expectedDocumentsCount);

            foreach (Document expectedDocument in expectedDocuments)
            {
                string expectedDocumentControlNumber = expectedDocument.ControlNumber;

                //We expect only few items so don't need to worry about performance
                Document actualDocument = actualDocuments.FirstOrDefault(document => document.ControlNumber == expectedDocumentControlNumber);

                Assert.That(actualDocument, Is.Not.Null, "Could not find document with control number {0}.", expectedDocumentControlNumber);

                ValidateDocument(expectedDocument, actualDocument);
            }
        }

        protected virtual void ValidateDocument(Document expectedDocument, Document actualDocument)
        {
            foreach (IDocumentValidator documentValidator in DocumentValidators)
            {
                documentValidator.ValidateDocument(actualDocument, expectedDocument);
            }
        }
    }
}