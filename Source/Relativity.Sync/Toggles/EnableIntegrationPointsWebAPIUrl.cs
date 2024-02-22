using Relativity.Toggles;

namespace Relativity.Sync.Toggles
{
	/// <summary>
	/// Enables using the IntegrationPointsWebAPIUrl
	/// </summary>
	[Description("Enables using the IntegrationPointsWebAPIUrl", "")]
	[DefaultValue(true)]
	[ExpectedRemovalDate(2024, 12, 30)]
	public class EnableIntegrationPointsWebAPIUrl : IToggle
	{
	}
}