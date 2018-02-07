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

		public virtual void ValidateDocument(Document actualDocument, Document expectedDocument)
		{
			FieldValue documentControlNumber = actualDocument[TestConstants.FieldNames.CONTROL_NUMBER];

			Assert.That(_documentProperty(actualDocument), Is.EqualTo(_documentProperty(expectedDocument)), "Actual field value is different than expected. Document control number: {0}", documentControlNumber);
		}
	}
}