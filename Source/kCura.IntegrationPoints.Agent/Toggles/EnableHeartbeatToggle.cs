using Relativity.Toggles;

namespace kCura.IntegrationPoints.Agent.Toggles
{
    [DefaultValue(true)]
    [Description("Enable Heartbeat update in SQL Queue Table", "Adler Sieben")]
    public class EnableHeartbeatToggle : IToggle
    {
    }
}
