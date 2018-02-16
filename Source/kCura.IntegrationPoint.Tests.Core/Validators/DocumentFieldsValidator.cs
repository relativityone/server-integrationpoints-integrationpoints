namespace kCura.IntegrationPoint.Tests.Core.Validators
{
	using NUnit.Framework;
	using Relativity.Client.DTOs;
	using TestHelpers;

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

			FieldValue documentControlNumber = destinationDocument[TestConstants.FieldNames.CONTROL_NUMBER];

			foreach (string fieldName in _fieldsToValidate)
			{
				FieldValue actualFieldValue = destinationDocument[fieldName];
				FieldValue expectedFieldValue = sourceDocument[fieldName];

				Assert.That(actualFieldValue.Value, Is.EqualTo(expectedFieldValue.Value),
					"Actual field value is different than expected. Field name {0}. Document control number {1}.", actualFieldValue.Name, documentControlNumber);
			}
		}
	}
}