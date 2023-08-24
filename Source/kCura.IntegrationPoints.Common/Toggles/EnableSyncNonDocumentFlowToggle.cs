using Relativity.Toggles;

namespace kCura.IntegrationPoints.Common.Toggles
{
	/// <summary>
	/// Enable Integration Points to use Non-document objects Sync workflow
	/// </summary>
	[DefaultValue(true)]
	[ExpectedRemovalDate(2023, 09, 30)]
    [Description("Enable Integration Points to use Non-document objects Sync workflow", "Adler Sieben")]
    public class EnableSyncNonDocumentFlowToggle : IToggle
    {
    }
}
