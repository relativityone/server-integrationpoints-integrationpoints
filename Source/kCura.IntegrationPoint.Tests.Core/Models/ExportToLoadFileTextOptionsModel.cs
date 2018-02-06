namespace kCura.IntegrationPoint.Tests.Core.Models
{
	using System.ComponentModel;

	public class ExportToLoadFileTextOptionsModel
	{
		[DefaultValue("Unicode")]
		public string TextFileEncoding { get; set; }

		public string TextPrecedence { get; set; }

		[DefaultValue("TEXT")]
		public string TextSubdirectoryPrefix { get; set; }
	}
}