using Relativity.Toggles;

namespace kCura.IntegrationPoints.Agent.Sync
{
	[DefaultValue(false)]
	[Description("Force Intergration Points to use the new Relativity Sync workflow.", "Codigo o Plomo")]
	internal class EnableSyncToggle : IToggle
	{
	}
}
