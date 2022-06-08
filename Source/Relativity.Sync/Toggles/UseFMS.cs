using Relativity.Toggles;

namespace Relativity.Sync.Toggles
{
    /// <summary>
    /// Toggle can be used to enable or disable File Movement Service (ADLS) for copying native files. 
    /// </summary>
    [DefaultValue(false)]
    [Description("When true, forces Sync to use File Movement Service to copy native files between workspaces.", "Adler Sieben")]
    public class UseFMS : IToggle
    {
        
    }
}