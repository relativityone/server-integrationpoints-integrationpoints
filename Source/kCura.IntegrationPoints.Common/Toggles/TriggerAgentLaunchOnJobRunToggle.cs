using Relativity.Toggles;

namespace kCura.IntegrationPoints.Common.Toggles
{
    [Description("When enabled, Integration Points will start new Agent by kepler call immediately after job run or retry", "Adler Sieben")]
    internal class TriggerAgentLaunchOnJobRunToggle : IToggle
    {
    }
}
