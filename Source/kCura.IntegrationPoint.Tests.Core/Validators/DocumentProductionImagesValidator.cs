using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;
using Relativity.Core.DTO;
using Document = kCura.Relativity.Client.DTOs.Document;

namespace kCura.IntegrationPoint.Tests.Core.Validators
{
	public class DocumentProductionImagesValidator : IDocumentValidator
	{
		private readonly IProductionImagesService _productionImagesService;
		private readonly int _destinationWorkspaceId;
		private readonly bool _expectInRepository;

		public DocumentProductionImagesValidator(IProductionImagesService productionImagesService, int destinationWorkspaceId, bool expectInRepository)
		{
			_productionImagesService = productionImagesService;	
			_destinationWorkspaceId = destinationWorkspaceId;
			_expectInRepository = expectInRepository;
		}

		public void ValidateDocument(Document destinationDocument, Document sourceDocument)
		{
			IList<File> destinationImages = _productionImagesService.GetProductionImagesFileInfo(_destinationWorkspaceId, destinationDocument.ArtifactID);
			Assert.That(destinationImages, Is.Not.Null, $"Could not find production image file for document {destinationDocument.ArtifactID}");

			int expectedNumberOfImages = Math.Max(sourceDocument.RelativityImageCount.GetValueOrDefault(), 1);
			Assert.That(destinationImages.Count, Is.EqualTo(expectedNumberOfImages), $"Number of produced images is different than expected for document {destinationDocument.ArtifactID}");

			foreach (File destinationImage in destinationImages)
			{
				Assert.That(destinationImage.InRepository, Is.EqualTo(_expectInRepository), $"Destination production image {destinationImage.FileID} does not have InRepository flag set to {_expectInRepository}");
			}
		}
	}
}