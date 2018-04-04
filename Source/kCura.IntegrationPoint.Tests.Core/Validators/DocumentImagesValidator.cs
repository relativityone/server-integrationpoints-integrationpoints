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
			if (sourceDocument.RelativityImageCount.GetValueOrDefault() > 0)
			{
				Assert.AreEqual(sourceDocument.RelativityImageCount, destinationDocument.RelativityImageCount);
			}
			else
			{
				Assert.AreEqual(1, destinationDocument.RelativityImageCount);
			}

			IList<File> destinationImages = _imagesService.GetImagesFileInfo(_destinationWorkspaceId, destinationDocument.ArtifactID);

			Assert.That(destinationImages, Is.Not.Null, $"Could not find file for document {destinationDocument.ArtifactID}");
			foreach (File destinationImage in destinationImages)
			{
				Assert.That(destinationImage.InRepository, Is.EqualTo(_expectInRepository), $"Destination image {destinationImage.FileID} does not have InRepository flag set to {_expectInRepository}");
			}
			
		}
	}
}