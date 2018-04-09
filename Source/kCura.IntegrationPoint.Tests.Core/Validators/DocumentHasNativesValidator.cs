namespace kCura.IntegrationPoint.Tests.Core.Validators
{
	using NUnit.Framework;
	using Relativity.Client.DTOs;

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

			Assert.That(destinationDocument.HasNative, Is.EqualTo(shouldExpectNatives));
		}

		protected  virtual bool ShouldExpectNativesForDocument(Document expectedDocument)
		{
			return ExpectHasNatives ?? expectedDocument.HasNative.GetValueOrDefault();
		}
	}
}