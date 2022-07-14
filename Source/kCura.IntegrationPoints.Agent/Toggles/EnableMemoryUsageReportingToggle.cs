using Relativity.Toggles;

namespace kCura.IntegrationPoints.Agent.Toggles
{
    [DefaultValue(true)]
    [Description("Enable APM/Splunk Memory Usage Reporting", "Adler Sieben")]
    internal class EnableMemoryUsageReportingToggle : IToggle
    {
    }
}
