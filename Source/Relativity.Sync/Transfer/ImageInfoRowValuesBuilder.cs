using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal sealed class ImageInfoRowValuesBuilder : IImageSpecialFieldRowValuesBuilder
	{
		private readonly IAntiMalwareHandler _antiMalwareHandler;

		public IDictionary<int, ImageFile[]> DocumentToImageFiles { get; }

        public ImageInfoRowValuesBuilder(IDictionary<int, ImageFile[]> documentToImageFiles, IAntiMalwareHandler antiMalwareHandler)
        {
            DocumentToImageFiles = documentToImageFiles;
            _antiMalwareHandler = antiMalwareHandler;
        }

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

			foreach(var imageFile in imagesForDocument)
            {
				imageFile.ValidateMalwareAsync(_antiMalwareHandler).GetAwaiter().GetResult();
				if (imageFile.IsMalwareDetected)
				{
					throw new SyncItemLevelErrorException($"File contains a virus or potentially unwanted software - File: {imageFile.Location}");
				}
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

		private static string GetIdentifierForImage(string documentIdentifier, int imageIndex, int numberOfDigits)
		{
			if (imageIndex == 0)
			{
				return documentIdentifier;
			}

			string indexSuffix = imageIndex.ToString().PadLeft(numberOfDigits, '0');
			return $"{documentIdentifier}_{indexSuffix}";
		}

		private static int GetNumberOfDigits(int totalImageCount)
		{
			return (int)Math.Ceiling(Math.Log10(totalImageCount));
		}
	}
}
