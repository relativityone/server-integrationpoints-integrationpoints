using Relativity.Toggles;

namespace kCura.IntegrationPoints.Common.Toggles
{
	/// <summary>
	/// When enabled, Integration Points will use new Relativity Sync application to run workspace-to-workspace workflows
	/// </summary>
	[DefaultValue(true)]
	[ExpectedRemovalDate(2023, 09, 30)]
    [Description("When enabled, Integration Points will use new Relativity Sync application to run workspace-to-workspace workflows", "Adler Sieben")]
    public class EnableRelativitySyncApplicationToggle : IToggle
    {
    }
}
