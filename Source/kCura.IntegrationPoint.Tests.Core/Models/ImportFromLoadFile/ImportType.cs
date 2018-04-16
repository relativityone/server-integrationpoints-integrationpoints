using System.ComponentModel;

namespace kCura.IntegrationPoint.Tests.Core.Models.ImportFromLoadFile
{
	public enum ImportType
	{
		[Description("Document Load File")]
		DocumentLoadFile,

		[Description("Image Load File")]
		ImageLoadFile,

		[Description("Production Load File")]
		ProductionLoadFile
	}
}
