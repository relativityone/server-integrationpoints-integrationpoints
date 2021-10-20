using Relativity.Toggles;

namespace kCura.IntegrationPoints.Agent.Toggles
{
    [DefaultValue(false)]
    [Description("Force Integration Points to use Non-document objects Sync workflow", "Adler Sieben")]
    public class EnableSyncNonDocumentFlowToggle : IToggle
    {
    }
}
