using Relativity.Toggles;

namespace Relativity.Sync.Toggles
{
    /// <summary>
    ///     Disable to run IAPI jobs without inserting information to audit.
    /// </summary>
    [DefaultValue(true)]
    [Description("Disable to run IAPI jobs without inserting information to audit.", "Adler Sieben")]
    public class EnableAuditToggle : IToggle
    {
    }
}
