using Relativity.Toggles;

namespace kCura.IntegrationPoints.Core.Toggles
{
	[DefaultValue(false)]
	[Description("Force Integration Points to use keplerized Import API with Relativity.DataTransfer.Legacy.SDK ", "Adler Sieben")]
	public class EnableKeplerizedImportAPIToggle : IToggle
	{
	}
}
