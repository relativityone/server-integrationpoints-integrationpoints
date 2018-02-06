namespace kCura.IntegrationPoint.Tests.Core.Models
{
	using System.ComponentModel;

	public class ExportToLoadFileOutputSettingsModel
	{
		public ExportToLoadFileLoadFileOptionsModel LoadFileOptions { get; set; } = new ExportToLoadFileLoadFileOptionsModel();
		public ExportToLoadFileImageOptionsModel ImageOptions { get; set; } = new ExportToLoadFileImageOptionsModel();
		public ExportToLoadFileNativeOptionsModel NativeOptions { get; set; } = new ExportToLoadFileNativeOptionsModel();
		public ExportToLoadFileTextOptionsModel TextOptions { get; set; } = new ExportToLoadFileTextOptionsModel();
	}
}