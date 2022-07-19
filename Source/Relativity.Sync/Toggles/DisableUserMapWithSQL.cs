using Relativity.Toggles;

namespace Relativity.Sync.Toggles
{
    /// <summary>
    ///     Toggle for disabling User Map with SQL when the workspace was restored.
    /// </summary>
    [DefaultValue(false)]
    [Description("When true, disables temporary workaround for mapping the users when the workspace was restored in push between workspaces job", "Adler Sieben")]
    public class DisableUserMapWithSQL : IToggle
    {
    }
}
