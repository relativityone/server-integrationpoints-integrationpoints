using Relativity.Toggles;

namespace kCura.IntegrationPoints.Common.Toggles
{
    /// <summary>
    /// Enables or disables transfer of files using CAL in legacy TAPI.
    /// </summary>
    [Description("When enabled, legacy TAPI will use CAL to transfer files.", "RIP and SFU")]
    public class UseCalInLegacyTapiToggle : IToggle
    {
    }
}
