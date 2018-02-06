namespace kCura.IntegrationPoint.Tests.Core.Models
{
	using System.ComponentModel;

	public class ExportToLoadFileNativeOptionsModel
	{
		[DefaultValue("NATIVE")]
		public string NativeSubdirectoryPrefix { get; set; }
	}
}