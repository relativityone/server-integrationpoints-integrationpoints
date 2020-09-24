using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Relativity.Sync.WorkspaceGenerator.Import;

namespace Relativity.Sync.WorkspaceGenerator.FileGenerating
{
	internal class ImageGenerator : IImageGenerator
	{
		private static readonly long KbMultiplier = 1024;
		private FileInfo[] _availableImages;
		private FileInfo[] _imagesPerDocument;

		public ImageGenerator(int totalImageSizeInMB, int numberOfDocuments)
		{
			_availableImages = new DirectoryInfo("Resources/Images").GetFiles();

			long KbavailableSum = _availableImages.Select(x => x.Length).Sum() / KbMultiplier;

			long KbPerDocument = (totalImageSizeInMB * KbMultiplier) / numberOfDocuments;

			int repeatSet = (int) Math.Max(KbPerDocument / KbavailableSum, 1);

			_imagesPerDocument = Enumerable.Repeat(_availableImages, repeatSet).SelectMany(x => x).ToArray();
		}

		public IEnumerable<ImageFileDTO> GetImagesForDocument(Document document)
		{
			return _imagesPerDocument.Select((x, i) =>
				new ImageFileDTO(document.Identifier, x.FullName, $"{i}{Path.GetExtension(x.Name)}", $"{document.Identifier}_{i}"));
		}

		public int SetPerDocumentCount => _imagesPerDocument.Length;
	}
}