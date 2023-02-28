using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoint.Tests.Core.TestHelpers.Dto;
using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.Validators
{
    public class DocumentImagesValidator : IDocumentValidator
    {
        private readonly IImagesService _imagesService;
        private readonly int _sourceWorkspaceId;
        private readonly int _destinationWorkspaceId;
        private readonly bool _expectInRepository;

        public DocumentImagesValidator(IImagesService imagesService, int sourceWorkspaceId, int destinationWorkspaceId, bool expectInRepository)
        {
            _imagesService = imagesService;
            _sourceWorkspaceId = sourceWorkspaceId;
            _destinationWorkspaceId = destinationWorkspaceId;
            _expectInRepository = expectInRepository;
        }

        public void ValidateDocument(Document destinationDocument, Document sourceDocument)
        {
            int expectedNumberOfImages = Math.Max(sourceDocument.ImageCount.GetValueOrDefault(), 1);
            Assert.That(destinationDocument.ImageCount, Is.EqualTo(expectedNumberOfImages), $"Number of images is different than expected for document {destinationDocument.ArtifactId}");

            IList<FileTestDto> sourceImages = _imagesService.GetImagesFileInfo(_sourceWorkspaceId, sourceDocument.ArtifactId);
            IList<FileTestDto> destinationImages = _imagesService.GetImagesFileInfo(_destinationWorkspaceId, destinationDocument.ArtifactId);

            Assert.That(destinationImages, Is.Not.Null, $"Could not find file for document {destinationDocument.ArtifactId}");
            foreach (FileTestDto destinationImage in destinationImages)
            {
                Assert.That(destinationImage.InRepository, Is.EqualTo(_expectInRepository), $"Destination image {destinationImage.Filename} does not have InRepository flag set to {_expectInRepository}");

                FileTestDto correspondingSourceImage = sourceImages.FirstOrDefault(x => x.Identifier == destinationImage.Identifier);

                if (correspondingSourceImage != null)
                {
                    destinationImage.Filename.Should().Be(correspondingSourceImage.Filename,
                        "Filename should be set for pushed images");
                }
            }
        }
    }
}
