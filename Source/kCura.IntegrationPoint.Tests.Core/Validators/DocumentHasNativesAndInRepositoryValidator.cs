using kCura.IntegrationPoint.Tests.Core.TestHelpers.Dto;

namespace kCura.IntegrationPoint.Tests.Core.Validators
{
    using NUnit.Framework;
    using TestHelpers;

    public class DocumentHasNativesAndInRepositoryValidator : DocumentHasNativesValidator
    {
        private readonly INativesService _nativesService;
        private readonly bool _expectInRepository;
        private readonly int _sourceWorkspaceId;
        private readonly int _destinationWorkspaceId;

        public DocumentHasNativesAndInRepositoryValidator(
            INativesService nativesService,
            int sourceWorkspaceId,
            int destinationWorkspaceId,
            bool expectHasNatives,
            bool expectInRepository) : base(expectHasNatives)
        {
            _destinationWorkspaceId = destinationWorkspaceId;
            _sourceWorkspaceId = sourceWorkspaceId;
            _nativesService = nativesService;
            _expectInRepository = expectInRepository;
        }

        public override void ValidateDocument(Document destinationDocument, Document sourceDocument)
        {
            base.ValidateDocument(destinationDocument, sourceDocument);

            FileTestDto destinationFile = _nativesService.GetNativeFileInfo(_destinationWorkspaceId, destinationDocument.ArtifactId);

            if (!ShouldExpectNativesForDocument(sourceDocument))
            {
                Assert.IsNull(destinationFile, $"There should be no natives in repository for document {destinationDocument.ArtifactId}");
                return;
            }

            FileTestDto sourceFile = _nativesService.GetNativeFileInfo(_sourceWorkspaceId, sourceDocument.ArtifactId);

            if (_expectInRepository)
            {
                Assert.That(destinationFile.Location, Is.Not.EqualTo(sourceFile.Location));
            }
            else
            {
                Assert.That(destinationFile.Location, Is.EqualTo(sourceFile.Location));
            }

            Assert.That(destinationFile, Is.Not.Null, $"Could not find file for document {destinationDocument.ArtifactId}");
            Assert.That(destinationFile.InRepository, Is.EqualTo(_expectInRepository));
        }
    }
}
