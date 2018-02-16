namespace kCura.IntegrationPoint.Tests.Core.Validators
{
	using System;
	using NUnit.Framework;
	using Relativity.Client.DTOs;
	using TestHelpers;

	public class DocumentPropertyValidator<T> : IDocumentValidator
	{
		private readonly Func<Document, T> _documentProperty;

		public DocumentPropertyValidator(Func<Document, T> documentProperty)
		{
			_documentProperty = documentProperty;
		}

		public virtual void ValidateDocument(Document destinationDocument, Document sourceDocument)
		{
			FieldValue documentControlNumber = destinationDocument[TestConstants.FieldNames.CONTROL_NUMBER];

			Assert.That(_documentProperty(destinationDocument), Is.EqualTo(_documentProperty(sourceDocument)), "Actual field value is different than expected. Document control number: {0}", documentControlNumber);
		}
	}
}