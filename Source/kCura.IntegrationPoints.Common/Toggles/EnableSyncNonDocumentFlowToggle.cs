using Relativity.Toggles;

namespace kCura.IntegrationPoints.Common.Toggles
{
    [DefaultValue(false)]
    [Description("Enable Integration Points to use Non-document objects Sync workflow", "Adler Sieben")]
    public class EnableSyncNonDocumentFlowToggle : IToggle
    {
    }
}
