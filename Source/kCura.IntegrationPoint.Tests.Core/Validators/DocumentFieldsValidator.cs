using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.Validators
{
    public class DocumentFieldsValidator : IDocumentValidator
    {
        private readonly string[] _fieldsToValidate;

        public DocumentFieldsValidator(params string[] fieldsToValidate)
        {
            _fieldsToValidate = fieldsToValidate;
        }

        public virtual void ValidateDocument(Document destinationDocument, Document sourceDocument)
        {
            if (_fieldsToValidate == null)
            {
                return;
            }

            foreach (string fieldName in _fieldsToValidate)
            {
                object actualFieldValue = destinationDocument[fieldName];
                object expectedFieldValue = sourceDocument[fieldName];

                Assert.That(actualFieldValue, Is.EqualTo(expectedFieldValue),
                    "Actual field value is different than expected. Field name: '{0}'. Document control number: '{1}'.",
                    fieldName, destinationDocument.ControlNumber);
            }
        }
    }
}
