namespace kCura.IntegrationPoint.Tests.Core.Validators
{
	using global::Relativity.Core.DTO;
	using NUnit.Framework;
	using TestHelpers;

	public class DocumentNativesAndInRepositoryValidator : DocumentNativesValidator
	{
		private readonly INativesService _nativesService;
		private readonly bool _expectInRepository;

		public DocumentNativesAndInRepositoryValidator(INativesService nativesService, bool expectHasNatives, bool expectInRepository) : base(expectHasNatives)
		{
			_nativesService = nativesService;
			_expectInRepository = expectInRepository;
		}

		public override void ValidateDocument(Relativity.Client.DTOs.Document actualDocument, Relativity.Client.DTOs.Document expectedDocument)
		{
			base.ValidateDocument(actualDocument, expectedDocument);

			File file = _nativesService.GetNativeFileInfo(actualDocument.ArtifactID);

			if (!ShouldExpectNativesForDocument(expectedDocument))
			{
				Assert.That(file, Is.Null, $"There should be no natives in repository for document {actualDocument.ArtifactID}");
				return;
			}

			Assert.That(file, Is.Not.Null, $"Could not find file for document {actualDocument.ArtifactID}");
			Assert.That(file.InRepository, Is.EqualTo(_expectInRepository));
		}
	}
}