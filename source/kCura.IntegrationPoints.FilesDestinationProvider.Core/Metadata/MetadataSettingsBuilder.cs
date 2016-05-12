using System.Collections.Generic;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Metadata
{
	class MetadataSettingsBuilder
	{
		private MetadataFileType _type;
		private readonly List<HeaderMetadata> _headers = new List<HeaderMetadata>();
		

		MetadataSettingsBuilder With(MetadataFileType type)
		{
			_type = type;
			return this;
		}

		MetadataSettingsBuilder With(HeaderMetadata header)
		{
			_headers.Add(header);
			return this;
		}

		MetadataSettings Create()
		{
			return new MetadataSettings()
			{
				HeaderMetadata = _headers,
				Type = _type
			};
		}
	}
}
