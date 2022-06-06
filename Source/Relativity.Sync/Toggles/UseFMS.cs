using Relativity.Toggles;

namespace Relativity.Sync.Toggles
{
    /// <summary>
    ///     Toggle for disabling User Map with SQL when the workspace was restored.
    /// </summary>
    [DefaultValue(false)]
    [Description("When true, forces Sync to use File Movement Service to copy native files between workspaces.", "Adler Sieben")]
    public class UseFMS : IToggle
    {
        
    }
}