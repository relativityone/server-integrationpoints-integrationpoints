using Relativity.Toggles;

namespace kCura.IntegrationPoints.Common.Toggles
{
    [DefaultValue(false)]
    [Description("When enabled, Integration Points will use new Relativity Sync application to run workspace-to-workspace workflows", "Adler Sieben")]
    public class EnableRelativitySyncApplicationToggle : IToggle
    {
    }
}
