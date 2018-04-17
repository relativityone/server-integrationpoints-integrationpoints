using kCura.IntegrationPoint.Tests.Core.Models.Shared;

namespace kCura.IntegrationPoint.Tests.Core.Models.ImportFromLoadFile
{
	public class ImportLoadFileImageProductionSettingsModel
	{
		public Numbering Numbering { get; set; }

		public OverwriteType ImportMode { get; set; }

		public bool CopyFilesToDocumentRepository { get; set; }
		
		public bool LoadExtractedText { get; set; }

		public string Production { get; set; }
	}
}
