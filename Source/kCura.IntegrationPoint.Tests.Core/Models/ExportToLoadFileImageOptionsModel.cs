namespace kCura.IntegrationPoint.Tests.Core.Models
{
	using System.ComponentModel;

	public class ExportToLoadFileImageOptionsModel
	{
		[DefaultValue("Single page TIFF/JPEG")]
		public string ImageFileType { get; set; }

		[DefaultValue("Original Images")]
		public string ImagePrecedence { get; set; }

		[DefaultValue("IMG")]
		public string ImageSubdirectoryPrefix { get; set; }
	}
}