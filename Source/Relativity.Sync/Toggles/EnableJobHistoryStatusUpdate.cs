using Relativity.Toggles;

namespace Relativity.Sync.Toggles
{
    /// <summary>
    /// When enabled, Sync will update Job History status.
    /// </summary>
    [DefaultValue(false)]
    [Description("When enabled, Sync will update Job History status.", "Adler Sieben")]
    public class EnableJobHistoryStatusUpdate : IToggle
    {
        
    }
}