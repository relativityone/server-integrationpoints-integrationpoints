using System.Text;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Metadata.Formatters
{
	public class ConcordanceFormatter : IMetadataFormatter
	{
		public string GetHeaders(MetadataSettings settings)
		{
			StringBuilder headerLine = new StringBuilder();
			settings.HeaderMetadata.ForEach(header => headerLine.Append($"{settings.QuoteDelimiter}{header.DisplayName}{settings.QuoteDelimiter}"));
			return headerLine.ToString();
		}
	}
}
