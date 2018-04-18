using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;
using Relativity.Core.DTO;
using Document = kCura.Relativity.Client.DTOs.Document;

namespace kCura.IntegrationPoint.Tests.Core.Validators
{
	public class DocumentImagesValidator : IDocumentValidator
	{
		private readonly IImagesService _imagesService;
		private readonly int _destinationWorkspaceId;
		private readonly bool _expectInRepository;

		public DocumentImagesValidator(IImagesService imagesService, int destinationWorkspaceId, bool expectInRepository)
		{
			_imagesService = imagesService;
			_destinationWorkspaceId = destinationWorkspaceId;
			_expectInRepository = expectInRepository;
		}

		public void ValidateDocument(Document destinationDocument, Document sourceDocument)
		{
			int expectedNumberOfImages = Math.Max(sourceDocument.RelativityImageCount.GetValueOrDefault(), 1);
			Assert.That(destinationDocument.RelativityImageCount, Is.EqualTo(expectedNumberOfImages), $"Number of images is different than expected for document {destinationDocument.ArtifactID}");

			IList<File> destinationImages = _imagesService.GetImagesFileInfo(_destinationWorkspaceId, destinationDocument.ArtifactID);

			Assert.That(destinationImages, Is.Not.Null, $"Could not find file for document {destinationDocument.ArtifactID}");
			foreach (File destinationImage in destinationImages)
			{
				Assert.That(destinationImage.InRepository, Is.EqualTo(_expectInRepository), $"Destination image {destinationImage.FileID} does not have InRepository flag set to {_expectInRepository}");
			}
			
		}
	}
}