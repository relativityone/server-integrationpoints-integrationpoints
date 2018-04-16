namespace kCura.IntegrationPoint.Tests.Core.Models.ImportFromLoadFile
{
	public class ImportLoadFileSettingsModel
	{
		public ImportType ImportType { get; set; }

		public string WorkspaceDestinationFolder { get; set; }

		public string ImportSource { get; set; }

		public int StartLine { get; set; }
	}
}
