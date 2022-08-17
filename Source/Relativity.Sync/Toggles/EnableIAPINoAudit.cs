using Relativity.Toggles;

namespace Relativity.Sync.Toggles
{
    /// <summary>
    ///     Enable to run IAPI jobs without inserting information to audit.
    /// </summary>
    [DefaultValue(false)]
    [Description("Enable to run IAPI jobs without inserting information to audit.", "Adler Sieben")]
    public class EnableIAPINoAudit : IToggle
    {
    }
}
