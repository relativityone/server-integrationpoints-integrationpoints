using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Relativity.Sync.Transfer
{
	internal sealed class ImageInfoRowValuesBuilder : IImageSpecialFieldRowValuesBuilder
	{
		public IDictionary<int, IEnumerable<ImageFile>> DocumentToImageFiles { get; private set; }

		public ImageInfoRowValuesBuilder(IDictionary<int, IEnumerable<ImageFile>> documentToImageFiles)
		{
			DocumentToImageFiles = documentToImageFiles;
		}

		public IEnumerable<SpecialFieldType> AllowedSpecialFieldTypes => new []
		{
			SpecialFieldType.ImageFileName,
			SpecialFieldType.ImageFileLocation
		};

		public IEnumerable<object> BuildRowValues(FieldInfoDto fieldInfoDto, RelativityObjectSlim document)
		{
			if(!DocumentToImageFiles.TryGetValue(document.ArtifactID, out IEnumerable<ImageFile> imagesForDocument) || !imagesForDocument.Any())
			{
				return Enumerable.Empty<object>();
			}

			switch (fieldInfoDto.SpecialFieldType)
			{
				case SpecialFieldType.ImageFileName:
					return imagesForDocument.Select(x => x.Filename);
				case SpecialFieldType.ImageFileLocation:
					return imagesForDocument.Select(x => x.Location);
				default:
					throw new ArgumentException($"Cannot build value for {nameof(SpecialFieldType)}.{fieldInfoDto.SpecialFieldType}.", nameof(fieldInfoDto));
			}
		}
	}
}
