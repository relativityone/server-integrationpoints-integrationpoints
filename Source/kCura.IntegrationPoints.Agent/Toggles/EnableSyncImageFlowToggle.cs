using Relativity.Toggles;

namespace kCura.IntegrationPoints.Agent.Toggles
{
	[DefaultValue(true)]
	[Description("Force Integration Points to use the new Relativity Sync workflow for images", "Adler Sieben")]
	internal class EnableSyncImageFlowToggle : IToggle
	{
	}
}