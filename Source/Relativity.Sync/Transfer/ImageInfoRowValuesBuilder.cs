using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
    internal sealed class ImageInfoRowValuesBuilder : IImageSpecialFieldRowValuesBuilder
    {
        private readonly IAntiMalwareHandler _antiMalwareHandler;

        public ImageInfoRowValuesBuilder(IDictionary<int, ImageFile[]> documentToImageFiles, IAntiMalwareHandler antiMalwareHandler)
        {
            DocumentToImageFiles = documentToImageFiles;
            _antiMalwareHandler = antiMalwareHandler;
        }

        public IDictionary<int, ImageFile[]> DocumentToImageFiles { get; }

        public IEnumerable<SpecialFieldType> AllowedSpecialFieldTypes => new[]
        {
            SpecialFieldType.ImageFileName,
            SpecialFieldType.ImageFileLocation,
            SpecialFieldType.ImageIdentifier
        };

        public IEnumerable<object> BuildRowsValues(FieldInfoDto fieldInfoDto, RelativityObjectSlim document, Func<RelativityObjectSlim, string> identifierFieldValueSelector)
        {
            if (!DocumentToImageFiles.TryGetValue(document.ArtifactID, out ImageFile[] imagesForDocument) || !imagesForDocument.Any())
            {
                return Enumerable.Empty<object>();
            }

            List<string> malwareFilePaths = new List<string>();
            foreach (var imageFile in imagesForDocument)
            {
                imageFile.ValidateMalwareAsync(_antiMalwareHandler).GetAwaiter().GetResult();
                if (imageFile.IsMalwareDetected)
                {
                    malwareFilePaths.Add(imageFile.Location);
                }
            }

            if (malwareFilePaths.Any())
            {
                StringBuilder sb = new StringBuilder();
                malwareFilePaths.ForEach(x => sb.AppendLine($"- {x},"));

                string malwareFilePathsMessage = $"File contains a virus or potentially unwanted software - Files:\n {sb}";

                throw new SyncItemLevelErrorException(malwareFilePathsMessage);
            }

            int numberOfDigits = GetNumberOfDigits(imagesForDocument.Length);
            string documentIdentifier = identifierFieldValueSelector(document);

            switch (fieldInfoDto.SpecialFieldType)
            {
                case SpecialFieldType.ImageFileName:
                    return imagesForDocument.Select(x => x.Filename);
                case SpecialFieldType.ImageFileLocation:
                    return imagesForDocument.Select(x => x.Location);
                case SpecialFieldType.ImageIdentifier:
                    return imagesForDocument.Select((image, i) =>
                        GetIdentifierForImage(documentIdentifier, i, numberOfDigits));
                default:
                    throw new ArgumentException($"Cannot build value for {nameof(SpecialFieldType)}.{fieldInfoDto.SpecialFieldType}.", nameof(fieldInfoDto));
            }
        }

        private string GetIdentifierForImage(string documentIdentifier, int imageIndex, int numberOfDigits)
        {
            if (imageIndex == 0)
            {
                return documentIdentifier;
            }

            string indexSuffix = imageIndex.ToString().PadLeft(numberOfDigits, '0');
            return $"{documentIdentifier}_{indexSuffix}";
        }

        private int GetNumberOfDigits(int totalImageCount)
        {
            return (int)Math.Ceiling(Math.Log10(totalImageCount));
        }
    }
}
