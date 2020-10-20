using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Relativity.Sync.Transfer
{
	internal sealed class ImageInfoRowValuesBuilder : IImageSpecialFieldRowValuesBuilder
	{
		public IDictionary<int, ImageFile[]> DocumentToImageFiles { get; }

		public ImageInfoRowValuesBuilder(IDictionary<int, ImageFile[]> documentToImageFiles)
		{
			DocumentToImageFiles = documentToImageFiles;
		}

		public IEnumerable<SpecialFieldType> AllowedSpecialFieldTypes => new []
		{
			SpecialFieldType.ImageFileName,
			SpecialFieldType.ImageFileLocation,
			SpecialFieldType.ImageIdentifier
		};

		public IEnumerable<object> BuildRowsValues(FieldInfoDto fieldInfoDto, RelativityObjectSlim document)
		{
			if(!DocumentToImageFiles.TryGetValue(document.ArtifactID, out ImageFile[] imagesForDocument) || !imagesForDocument.Any())
			{
				return Enumerable.Empty<object>();
			}

			switch (fieldInfoDto.SpecialFieldType)
			{
				case SpecialFieldType.ImageFileName:
					return imagesForDocument.Select(x => x.Filename);
				case SpecialFieldType.ImageFileLocation:
					return imagesForDocument.Select(x => x.Location);
				case SpecialFieldType.ImageIdentifier:
					return imagesForDocument.Select(x => x.Identifier);
				default:
					throw new ArgumentException($"Cannot build value for {nameof(SpecialFieldType)}.{fieldInfoDto.SpecialFieldType}.", nameof(fieldInfoDto));
			}
		}
	}
}
