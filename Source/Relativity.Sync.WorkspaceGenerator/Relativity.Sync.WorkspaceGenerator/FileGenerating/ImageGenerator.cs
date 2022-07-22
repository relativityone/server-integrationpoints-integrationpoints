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
        private readonly FileInfo[] _imagesPerDocument;

        public ImageGenerator(int totalImageSizeInMb, int numberOfDocuments)
        {
            FileInfo[] availableImages = new DirectoryInfo("Resources/Images").GetFiles();

            long availableImagesSizeSumInKb = availableImages.Select(x => x.Length).Sum() / KbMultiplier;

            long requestedPerDocumentImagesSizeInKb = (totalImageSizeInMb * KbMultiplier) / numberOfDocuments;

            int repeatSetCount = (int) Math.Max(requestedPerDocumentImagesSizeInKb / availableImagesSizeSumInKb, 1);

            _imagesPerDocument = Enumerable.Repeat(availableImages, repeatSetCount).SelectMany(x => x).ToArray();
        }

        public IEnumerable<ImageFileDTO> GetImagesForDocument(Document document)
        {
            return _imagesPerDocument.Select((x, i) =>
                new ImageFileDTO(document.Identifier, x.FullName, $"{i}{Path.GetExtension(x.Name)}", $"{document.Identifier}_{i}"));
        }

        public int SetPerDocumentCount => _imagesPerDocument.Length;
    }
}