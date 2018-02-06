namespace kCura.IntegrationPoint.Tests.Core.Models
{
	using System.ComponentModel;

	public class ExportToLoadFileDetailsModel
	{
		[DefaultValue(true)]
		public bool? LoadFile { get; set; }

		[DefaultValue(false)]
		public bool? ExportImages { get; set; }

		[DefaultValue(false)]
		public bool? ExportNatives { get; set; }

		[DefaultValue(false)]
		public bool? TextFieldsAsFiles { get; set; }

		public string DestinationFolder { get; set; }

		[DefaultValue(true)]
		public bool? CreateExportFolder { get; set; }

		[DefaultValue(false)]
		public bool? OverwriteFiles { get; set; }
	}
}