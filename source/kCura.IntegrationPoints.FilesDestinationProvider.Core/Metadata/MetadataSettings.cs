using System.Collections.Generic;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Metadata
{
	public struct MetadataSettings
	{
		public MetadataFileType Type { get; set; }
		public string FilePath { get; set; }

		public char QuoteDelimiter { get; set; }

		public List<HeaderMetadata> HeaderMetadata { get; set; }
	}
}
