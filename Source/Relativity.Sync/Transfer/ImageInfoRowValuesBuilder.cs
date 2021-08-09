using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Banzai;

namespace Relativity.Sync.Transfer
{
	internal sealed class ImageInfoRowValuesBuilder : IImageSpecialFieldRowValuesBuilder
	{
		public IDictionary<int, ImageFile[]> DocumentToImageFiles { get; }

		public ImageInfoRowValuesBuilder(IDictionary<int, ImageFile[]> documentToImageFiles)
		{
			DocumentToImageFiles = documentToImageFiles;
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
