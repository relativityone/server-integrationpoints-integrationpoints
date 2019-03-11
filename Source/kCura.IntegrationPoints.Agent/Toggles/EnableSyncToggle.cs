using Relativity.Toggles;

namespace kCura.IntegrationPoints.Agent.Toggles
{
	[DefaultValue(true)]
	[Description("Force Integration Points to use the new Relativity Sync workflow.", "Codigo o Plomo")]
	internal class EnableSyncToggle : IToggle
	{
	}
}
